using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.ChunkIterationPerformance
{
    [DisableAutoCreation]
    public class ChunkIterationJobSystem : JobComponentSystem
    {
        private NativeArray<double> _totalSum;
        private NativeArray<double> _noTagSum;
        private NativeArray<double> _firstTagSum;
        private NativeArray<double> _secondTagSum;

        [BurstCompile]
        struct SumChunkJob : IJobChunk
        {
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<double> TotalResults;
            [NativeDisableParallelForRestriction]
            public NativeArray<double> NoTagResults;
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<double> FirstTagResults;
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<double> SecondTagResults;
            [ReadOnly]
            public ArchetypeChunkComponentType<RandomValue> RandomType;
            [ReadOnly]
            public ArchetypeChunkComponentType<FirstTag> FirstTagType;
            [ReadOnly]
            public ArchetypeChunkComponentType<SecondTag> SecondTagType;

            public void Execute(ArchetypeChunk chunk, int index)
            {
                double sum = 0;
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


        private ComponentGroup _randomGroup;
        protected override void OnCreateManager()
        {
            _randomGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                Any = new ComponentType[0],
                All = new[] { ComponentType.Create<RandomValue>() },
                None = new ComponentType[0]
            });
        }

        protected override void OnDestroyManager()
        {
            base.OnDestroyManager();
            if (_totalSum.IsCreated)
            {
                _totalSum.Dispose();
                _noTagSum.Dispose();
                _firstTagSum.Dispose();
                _secondTagSum.Dispose();
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!_totalSum.IsCreated)
            {

                var chunks = EntityManager.CreateArchetypeChunkArray(new EntityArchetypeQuery
                {
                    Any = new ComponentType[0],
                    All = new[] { ComponentType.Create<RandomValue>() },
                    None = new ComponentType[0]
                }, Allocator.Persistent);
                int chunksCount = chunks.Length;
                chunks.Dispose();
                _totalSum = new NativeArray<double>(chunksCount, Allocator.Persistent);
                _noTagSum = new NativeArray<double>(chunksCount, Allocator.Persistent);
                _firstTagSum = new NativeArray<double>(chunksCount, Allocator.Persistent);
                _secondTagSum = new NativeArray<double>(chunksCount, Allocator.Persistent);
            }

            var job = new SumChunkJob
            {
                TotalResults = _totalSum,
                FirstTagResults = _firstTagSum,
                SecondTagResults = _secondTagSum,
                NoTagResults = _noTagSum,
                RandomType = GetArchetypeChunkComponentType<RandomValue>(true),
                FirstTagType = GetArchetypeChunkComponentType<FirstTag>(true),
                SecondTagType = GetArchetypeChunkComponentType<SecondTag>(true)
            }.Schedule(_randomGroup, inputDeps);
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
                secondTagSum += _secondTagSum[i];
            }
            InitializeChunkIterationWorld.LogSumResults(this, noTagsSum, firstTagSum, secondTagSum, totalSum);
            return inputDeps;
        }
    }
}
