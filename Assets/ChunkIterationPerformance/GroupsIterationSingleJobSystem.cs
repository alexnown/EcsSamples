using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.ChunkIterationPerformance
{
    [DisableAutoCreation]
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
        private NativeArray<double>[] _allocatedArrays;
        private NativeArray<JobHandle> _jobHandler;

        protected override void OnCreateManager()
        {
            _jobHandler = new NativeArray<JobHandle>(4, Allocator.Persistent);
            _allocatedArrays = new NativeArray<double>[4];
            _allocatedArrays[0] = new NativeArray<double>(1, Allocator.Persistent);
            _allocatedArrays[1] = new NativeArray<double>(1, Allocator.Persistent);
            _allocatedArrays[2] = new NativeArray<double>(1, Allocator.Persistent);
            _allocatedArrays[3] = new NativeArray<double>(1, Allocator.Persistent);

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
            if (_jobHandler.IsCreated) _jobHandler.Dispose();
            foreach (var array in _allocatedArrays)
            {
                if (array.IsCreated) array.Dispose();
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var sumNoTag = CreateJob(_noTagEntities, _allocatedArrays[0]);
            var sumFirstTag = CreateJob(_firstTagEntities, _allocatedArrays[1]);
            var sumSecodTag = CreateJob(_secondTagEntities, _allocatedArrays[2]);
            var sumAll = CreateJob(_allRandomEntities, _allocatedArrays[3]);

            _jobHandler[0] = sumAll.Schedule();
            _jobHandler[1] = sumNoTag.Schedule();
            _jobHandler[2] = sumFirstTag.Schedule();
            _jobHandler[3] = sumSecodTag.Schedule();

            JobHandle.CompleteAll(_jobHandler);

            double noTagsSum = sumNoTag.Sums[0];
            double firstTagSum = sumFirstTag.Sums[0];
            double secondTagSum = sumSecodTag.Sums[0];
            double totalSum = sumAll.Sums[0];

            InitializeChunkIterationWorld.LogSumResults(this, noTagsSum, firstTagSum, secondTagSum, totalSum);
            return base.OnUpdate(inputDeps);
        }

        private SummingElementsJob CreateJob(ComponentGroup group, NativeArray<double> array)
        {
            return new SummingElementsJob
            {
                Sums = array,
                RandomValues = group.GetComponentDataArray<RandomValue>()
            };
        }
    }
}
