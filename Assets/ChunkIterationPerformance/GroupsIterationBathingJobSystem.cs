using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.ChunkIterationPerformance
{
    [DisableAutoCreation]
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
        private NativeArray<JobHandle> _jobHandler;
        private NativeArray<double>[] _allocatedArrays;

        protected override void OnCreateManager()
        {
            _jobHandler = new NativeArray<JobHandle>(4, Allocator.Persistent);
            _allocatedArrays = new NativeArray<double>[4];
            _allocatedArrays[0] = new NativeArray<double>(BatchesCount, Allocator.Persistent);
            _allocatedArrays[1] = new NativeArray<double>(BatchesCount, Allocator.Persistent);
            _allocatedArrays[2] = new NativeArray<double>(BatchesCount, Allocator.Persistent);
            _allocatedArrays[3] = new NativeArray<double>(BatchesCount, Allocator.Persistent);

            _noTagEntities = GetComponentGroup(ComponentType.Create<RandomValue>(),
                ComponentType.Subtractive<FirstTag>(),
                ComponentType.Subtractive<SecondTag>());
            _firstTagEntities = GetComponentGroup(ComponentType.Create<RandomValue>(), ComponentType.Create<FirstTag>());
            _secondTagEntities = GetComponentGroup(ComponentType.Create<RandomValue>(),
                ComponentType.Create<SecondTag>());
            _allRandomEntities = GetComponentGroup(ComponentType.Create<RandomValue>());
        }

        protected override void OnDestroyManager()
        {
            base.OnDestroyManager();
            if(_jobHandler.IsCreated) _jobHandler.Dispose();
            foreach (var array in _allocatedArrays)
            {
                if(array.IsCreated) array.Dispose();
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var sumNoTag = CreateJob(_noTagEntities, _allocatedArrays[0]);
            var noTagHandler = sumNoTag.ScheduleBatch(_noTagEntities.CalculateLength(), sumNoTag.EntitiesInButch);

            var sumFirstTag = CreateJob(_firstTagEntities, _allocatedArrays[1]);
            var firstTagHandler = sumFirstTag.ScheduleBatch(_firstTagEntities.CalculateLength(), sumFirstTag.EntitiesInButch);

            var sumSecodTag = CreateJob(_secondTagEntities, _allocatedArrays[2]);
            var secondTagHandler = sumSecodTag.ScheduleBatch(_secondTagEntities.CalculateLength(), sumSecodTag.EntitiesInButch);

            var sumAll = CreateJob(_allRandomEntities, _allocatedArrays[3]);
            var handlerAll = sumAll.ScheduleBatch(_allRandomEntities.CalculateLength(), sumAll.EntitiesInButch);


            _jobHandler[0] = handlerAll;
            _jobHandler[1] = noTagHandler;
            _jobHandler[2] = firstTagHandler;
            _jobHandler[3] = secondTagHandler;

            JobHandle.CompleteAll(_jobHandler);

            double noTagsSum = CalcSum(sumNoTag.Sums);
            double firstTagSum = CalcSum(sumFirstTag.Sums);
            double secondTagSum = CalcSum(sumSecodTag.Sums);
            double totalSum = CalcSum(sumAll.Sums);

            InitializeChunkIterationWorld.LogSumResults(this, noTagsSum, firstTagSum, secondTagSum, totalSum);
            return base.OnUpdate(inputDeps);
        }

        private double CalcSum(NativeArray<double> array)
        {
            double sum = 0;
            for (int i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }
            return sum;
        }

        private BatchedSummElements CreateJob(ComponentGroup group, NativeArray<double> batchesSum)
        {
            int inBatch = _allRandomEntities.CalculateLength() / batchesSum.Length + 1;
            return new BatchedSummElements
            {
                EntitiesInButch = inBatch,
                Sums = batchesSum,
                RandomValues = group.GetComponentDataArray<RandomValue>()
            };
        }
    }
}
