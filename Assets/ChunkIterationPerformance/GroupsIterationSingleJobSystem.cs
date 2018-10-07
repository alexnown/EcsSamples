using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.ChunkIterationPerformance
{
    public class GroupsIterationSingleJobSystem : JobComponentSystem
    {
        #region Job
        [BurstCompile]
        struct SummingElementsJob : IJob
        {
            [NativeDisableParallelForRestriction]
            [WriteOnly]
            public NativeArray<double> Sums;
            [ReadOnly]
            public ComponentDataArray<RandomValue> RandomValues;
           
            public void Execute()
            {
                double sum = 0;
                for (int i = 0; i < RandomValues.Length; i++)
                {
                    sum += RandomValues[i].Value;
                }
                Sums[0] = sum;
            }
        }
        #endregion

        private ComponentGroup _noTagEntities;
        private ComponentGroup _firstTagEntities;
        private ComponentGroup _secondTagEntities;
        private ComponentGroup _allRandomEntities;

        protected override void OnCreateManager()
        {
            _noTagEntities = GetComponentGroup(ComponentType.Create<RandomValue>(),
                ComponentType.Subtractive<FirstTag>(),
                ComponentType.Subtractive<SecondTag>());
            _firstTagEntities = GetComponentGroup(ComponentType.Create<RandomValue>(), ComponentType.Create<FirstTag>());
            _secondTagEntities = GetComponentGroup(ComponentType.Create<RandomValue>(),
                ComponentType.Create<SecondTag>());
            _allRandomEntities = GetComponentGroup(ComponentType.Create<RandomValue>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handlersArray = new NativeList<JobHandle>(4, Allocator.TempJob);

            var sumNoTag = CreateJob(_noTagEntities);
            var sumFirstTag = CreateJob(_firstTagEntities);
            var sumSecodTag = CreateJob(_secondTagEntities);
            var sumAll = CreateJob(_allRandomEntities);

            handlersArray.Add(sumAll.Schedule());
            handlersArray.Add(sumNoTag.Schedule());
            handlersArray.Add(sumFirstTag.Schedule());
            handlersArray.Add(sumSecodTag.Schedule());
            
            JobHandle.CompleteAll(handlersArray);
            handlersArray.Dispose();

            double noTagsSum = sumNoTag.Sums[0];
            double firstTagSum = sumFirstTag.Sums[0];
            double secondTagSum = sumSecodTag.Sums[0];
            double totalSum = sumAll.Sums[0];

            sumNoTag.Sums.Dispose();
            sumFirstTag.Sums.Dispose();
            sumSecodTag.Sums.Dispose();
            sumAll.Sums.Dispose();

            InitializeChunkIterationWorld.LogSumResults(this, noTagsSum, firstTagSum, secondTagSum, totalSum);
            return base.OnUpdate(inputDeps);
        }
        
        private SummingElementsJob CreateJob(ComponentGroup group)
        {
            return new SummingElementsJob
            {
                Sums = new NativeArray<double>(1, Allocator.TempJob),
                RandomValues = group.GetComponentDataArray<RandomValue>()
            };
        }
    }
}
