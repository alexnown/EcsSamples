using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[DisableAutoCreation][AlwaysUpdateSystem]
public class NativeArrayJobSystem : JobComponentSystem
{
    [BurstCompile]
    public struct WriteToNativeArray : IJob
    {
        public NativeArray<int> Array;
        public int Iterations;

        public void Execute()
        {
            int value = 0;
            int arrayLenght = Array.Length;
            while (Iterations > 0)
            {
                Iterations--;
                value++;
                for (int i = 0; i < arrayLenght; i++)
                {
                    Array[i] = value;
                }
            }
        }
    }

    [BurstCompile]
    public struct ReadNativeArray : IJob
    {
        [ReadOnly]
        public NativeArray<int> Array;
        public int Iterations;

        public void Execute()
        {
            int value = 0;
            int arrayLenght = Array.Length;
            while (Iterations > 0)
            {
                Iterations--;
                for (int i = 0; i < arrayLenght; i++)
                {
                    value = Array[i];
                }
            }
        }
    }

    private NativeArray<int> _array; private NativeArray<int> _readedArray;
    protected override void OnCreateManager()
    {
        _array = new NativeArray<int>(InitNativeCollectionsSpeedComparing.ElementsCount, Allocator.Persistent);
        _readedArray = new NativeArray<int>(InitNativeCollectionsSpeedComparing.ElementsCount, Allocator.Persistent);
    }

    protected override void OnDestroyManager()
    {
        if (_array.IsCreated) _array.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        
        var job = new WriteToNativeArray
        {
            Array = _array,
            Iterations = InitNativeCollectionsSpeedComparing.Iterations
        }.Schedule();
        job.Complete(); 

        var readJob = new ReadNativeArray
        {
            Array = _readedArray,
            Iterations = InitNativeCollectionsSpeedComparing.Iterations
        }.Schedule();
        readJob.Complete();

        return inputDeps;
    }
}
