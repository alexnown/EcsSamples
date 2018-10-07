using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.ChunkIterationPerformance
{
    public class GroupsIterationBathingJobSystem : JobComponentSystem
    {
        public const int BatchesCount = 16;

        #region Job
        [BurstCompile]
        struct BatchedSummElements : IJobParallelForBatch
        {
            public int EntitiesInButch;
            [NativeDisableParallelForRestriction]
            [WriteOnly]
            public NativeArray<double> Sums;
            [ReadOnly]
            public ComponentDataArray<RandomValue> RandomValues;
            public void Execute(int startIndex, int count)
            {
                double sum = 0;
                for (int i = 0; i < count; i++)
                {
                    sum += RandomValues[startIndex + i].Value;
                }
                int index = startIndex / EntitiesInButch;
                Sums[index] = sum;
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

            var sumNoTag = CreateJob(_noTagEntities, BatchesCount);
            var noTagHandler = sumNoTag.ScheduleBatch(_noTagEntities.CalculateLength(), sumNoTag.EntitiesInButch);

            var sumFirstTag = CreateJob(_firstTagEntities, BatchesCount);
            var firstTagHandler = sumFirstTag.ScheduleBatch(_firstTagEntities.CalculateLength(), sumFirstTag.EntitiesInButch);

            var sumSecodTag = CreateJob(_secondTagEntities, BatchesCount);
            var secondTagHandler = sumSecodTag.ScheduleBatch(_secondTagEntities.CalculateLength(), sumSecodTag.EntitiesInButch);
            
            var sumAll = CreateJob(_allRandomEntities, BatchesCount);
            var handlerAll = sumAll.ScheduleBatch(_allRandomEntities.CalculateLength(), sumAll.EntitiesInButch);

            handlersArray.Add(handlerAll);
            handlersArray.Add(noTagHandler);
            handlersArray.Add(firstTagHandler);
            handlersArray.Add(secondTagHandler);

            JobHandle.CompleteAll(handlersArray);
            handlersArray.Dispose();

            double noTagsSum = GetSumAndDispose(sumNoTag.Sums);
            double firstTagSum = GetSumAndDispose(sumFirstTag.Sums);
            double secondTagSum = GetSumAndDispose(sumSecodTag.Sums);
            double totalSum = GetSumAndDispose(sumAll.Sums);

            InitializeChunkIterationWorld.LogSumResults(this, noTagsSum, firstTagSum, secondTagSum, totalSum);
            return base.OnUpdate(inputDeps);
        }

        private double GetSumAndDispose(NativeArray<double> array)
        {
            double sum = 0;
            for (int i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }
            array.Dispose();
            return sum;
        }

        private BatchedSummElements CreateJob(ComponentGroup group, int batchesCount)
        {
            int inBatch = _allRandomEntities.CalculateLength() / batchesCount + 1;
            return new BatchedSummElements
            {
                EntitiesInButch = inBatch,
                Sums = new NativeArray<double>(batchesCount, Allocator.TempJob),
                RandomValues = group.GetComponentDataArray<RandomValue>()
            };
        }
    }
}
