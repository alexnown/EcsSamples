/*
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;


namespace alexnown.ChunkIterationPerformance
{
[DisableAutoCreation]
    [Obsolete("Cant correct summ in different threads ")]
    public class ProcessTagsJobSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var firstSumJob = new ProcessFirstTag
            {
                Result = new NativeArray<double>(1, Allocator.TempJob)
            };
            var secondSumJob = new ProcessSecondTag
            {
                Result = new NativeArray<double>(1, Allocator.TempJob)
            };
            var allRandoms = new ProcessRandoms
            {
                Result = new NativeArray<double>(1, Allocator.TempJob)
            };

            var firstHandler = firstSumJob.Schedule(this);
            var secondHandler = secondSumJob.Schedule(this);
            var allRandomsHandler = allRandoms.Schedule(this);
            JobHandle.CompleteAll(ref firstHandler, ref secondHandler, ref allRandomsHandler);

            double firstSum = firstSumJob.Result[0];
            double secondSum = secondSumJob.Result[0];
            double allSum = allRandoms.Result[0];
            firstSumJob.Result.Dispose();
            secondSumJob.Result.Dispose();
            allRandoms.Result.Dispose();
            if (InitializeChunkIterationWorld.LogSystemResults)
            {
                UnityEngine.Debug.Log(nameof(SingleQueryChunkIterationSystem) + $" {0:F3}/{firstSum:F3}/{secondSum:F3} total = {allSum:F3}");
            }
            return base.OnUpdate(inputDeps);
        }

        private struct ProcessFirstTag : IJobProcessComponentData<FirstTag, RandomValue>
        {
            [NativeDisableParallelForRestriction]
            public NativeArray<double> Result;
            public void Execute([ReadOnly] ref FirstTag tag, [ReadOnly] ref RandomValue random)
            {
                Result[0] += random.Value;
            }
        }

        private struct ProcessSecondTag : IJobProcessComponentData<SecondTag, RandomValue>
        {
            [NativeDisableParallelForRestriction]
            public NativeArray<double> Result;
            public void Execute([ReadOnly] ref SecondTag tag, [ReadOnly] ref RandomValue random)
            {
                Result[0] += random.Value;
            }
        }

        private struct ProcessRandoms : IJobProcessComponentData<RandomValue>
        {
            [NativeDisableParallelForRestriction]
            public NativeArray<double> Result;
            public void Execute([ReadOnly] ref RandomValue random)
            {
                Result[0] += random.Value;
            }
        }
    }
} */