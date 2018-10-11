using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.ChunkIterationPerformance
{
    [DisableAutoCreation]
    public class ChunkIterationBatchingJobSystem : JobComponentSystem
    {
        private int BatchesCount = 64;
        private EntityArchetypeQuery _query;
        private NativeArray<double> _totalSum;
        private NativeArray<double> _noTagSum;
        private NativeArray<double> _firstTagSum;
        private NativeArray<double> _secondTimeSum;

        #region Job
        [BurstCompile]
        struct ChunkSum : IJobParallelForBatch
        {
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<double> TotalResults;
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<double> NoTagResults;
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<double> FirstTagResults;
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<double> SecondTagResults;
            [ReadOnly]
            public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly]
            public ArchetypeChunkComponentType<RandomValue> RandomType;
            [ReadOnly]
            public ArchetypeChunkComponentType<FirstTag> FirstTagType;
            [ReadOnly]
            public ArchetypeChunkComponentType<SecondTag> SecondTagType;

            [ReadOnly]
            public int ChunksInBatch;

            public void Execute(int startIndex, int count)
            {
                double noTagsSum = 0;
                double firstTagSum = 0;
                double secondTagSum = 0;
                double totalSum = 0;
                for (int i = startIndex; i < startIndex + count; i++)
                {
                    var chunk = Chunks[i];
                    if (chunk.Count == 0) continue;
                    bool hasFirst = chunk.Has(FirstTagType);
                    bool hasSecond = chunk.Has(SecondTagType);
                    var array = chunk.GetNativeArray(RandomType);
                    double totalArraySum = 0;
                    for (int j = 0; j < array.Length; j++)
                    {
                        totalArraySum += array[j].Value;
                    }
                    totalSum += totalArraySum;
                    if (hasFirst) firstTagSum += totalArraySum;
                    if (hasSecond) secondTagSum += totalArraySum;
                    else if (!hasFirst) noTagsSum += totalArraySum;
                }
                int resultIndex = startIndex / ChunksInBatch;
                TotalResults[resultIndex] = totalSum;
                FirstTagResults[resultIndex] = firstTagSum;
                SecondTagResults[resultIndex] = secondTagSum;
                NoTagResults[resultIndex] = noTagsSum;
            }
        }
        #endregion

        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            _query = new EntityArchetypeQuery
            {
                Any = new ComponentType[0],
                All = new[] { ComponentType.Create<RandomValue>() },
                None = new ComponentType[0]
            };
            _totalSum = new NativeArray<double>(BatchesCount, Allocator.Persistent);
            _noTagSum = new NativeArray<double>(BatchesCount, Allocator.Persistent);
            _firstTagSum = new NativeArray<double>(BatchesCount, Allocator.Persistent);
            _secondTimeSum = new NativeArray<double>(BatchesCount, Allocator.Persistent);
        }

        protected override void OnDestroyManager()
        {
            base.OnDestroyManager();
            _totalSum.Dispose();
            _noTagSum.Dispose();
            _firstTagSum.Dispose();
            _secondTimeSum.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var chunks = EntityManager.CreateArchetypeChunkArray(_query, Allocator.TempJob);
            InitializeChunkIterationWorld.LogChunksCount(this, chunks.Length);
            int inBatch = chunks.Length / BatchesCount + 1;
            var job = new ChunkSum
            {
                Chunks = chunks,
                RandomType = GetArchetypeChunkComponentType<RandomValue>(true),
                FirstTagType = GetArchetypeChunkComponentType<FirstTag>(true),
                SecondTagType = GetArchetypeChunkComponentType<SecondTag>(true),
                TotalResults = _totalSum,
                NoTagResults = _noTagSum,
                FirstTagResults = _firstTagSum,
                SecondTagResults = _secondTimeSum,
                ChunksInBatch = inBatch
            }.ScheduleBatch(chunks.Length, inBatch);
            job.Complete();
            double noTagsSum = 0;
            double firstTagSum = 0;
            double secondTagSum = 0;
            double totalSum = 0;
            for (int i = 0; i < _totalSum.Length; i++)
            {
                totalSum += _totalSum[i];
                noTagsSum += _noTagSum[i];
                firstTagSum += _firstTagSum[i];
                secondTagSum += _secondTimeSum[i];
            }

            InitializeChunkIterationWorld.LogSumResults(this, noTagsSum, firstTagSum, secondTagSum, totalSum);

            chunks.Dispose();
            return base.OnUpdate(inputDeps);
        }
    }
}
