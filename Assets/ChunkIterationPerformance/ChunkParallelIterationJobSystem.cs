using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.ChunkIterationPerformance
{
    [DisableAutoCreation]
    public class ChunkParallelIterationJobSystem : JobComponentSystem
    {
        private EntityArchetypeQuery _query;
        private NativeArray<double> _totalSum;
        private NativeArray<double> _noTagSum;
        private NativeArray<double> _firstTagSum;
        private NativeArray<double> _secondTimeSum;

        #region Job
        [BurstCompile]
        struct ChunkSum : IJobParallelFor
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
            public void Execute(int index)
            {
                double sum = 0;
                var chunk = Chunks[index];
                var array = chunk.GetNativeArray(RandomType);
                for (int i = 0; i < array.Length; i++)
                {
                    sum += array[i].Value;
                }
                TotalResults[index] = sum;
                bool hasFirst = chunk.Has(FirstTagType);
                bool hasSecond = chunk.Has(SecondTagType);
                if (hasFirst) FirstTagResults[index] = sum;
                if (hasSecond) SecondTagResults[index] = sum;
                else if (!hasFirst) NoTagResults[index] = sum;
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
        }

        protected override void OnDestroyManager()
        {
            base.OnDestroyManager();
            if (_totalSum.IsCreated)
            {
                _totalSum.Dispose();
                _noTagSum.Dispose();
                _firstTagSum.Dispose();
                _secondTimeSum.Dispose();
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var chunks = EntityManager.CreateArchetypeChunkArray(_query, Allocator.TempJob);
            InitializeChunkIterationWorld.LogChunksCount(this, chunks.Length);
            if (!_totalSum.IsCreated)
            {
                _totalSum = new NativeArray<double>(chunks.Length, Allocator.Persistent);
                _noTagSum = new NativeArray<double>(chunks.Length, Allocator.Persistent);
                _firstTagSum = new NativeArray<double>(chunks.Length, Allocator.Persistent);
                _secondTimeSum = new NativeArray<double>(chunks.Length, Allocator.Persistent);
            }
            var job = new ChunkSum
            {
                Chunks = chunks,
                RandomType = GetArchetypeChunkComponentType<RandomValue>(true),
                FirstTagType = GetArchetypeChunkComponentType<FirstTag>(true),
                SecondTagType = GetArchetypeChunkComponentType<SecondTag>(true),
                TotalResults = _totalSum,
                NoTagResults = _noTagSum,
                FirstTagResults = _firstTagSum,
                SecondTagResults = _secondTimeSum
            }.Schedule(chunks.Length, 64);
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
