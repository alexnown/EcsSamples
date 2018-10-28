using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[DisableAutoCreation][AlwaysUpdateSystem]
public class NativeIntArrayJobSystem : JobComponentSystem
{
    [BurstCompile]
    public struct WriteToNativeIntArray : IJob
    {
        [WriteOnly]
        public NativeConcurrentIntArray Array;
        public int Iterations;
        public int ArrayLenght;

        public void Execute()
        {
            int value = 0;
            while (Iterations > 0)
            {
                Iterations--;
                value++;
                for (int i = 0; i < ArrayLenght; i++)
                {
                    Array.SetValue(i, value);
                }
            }
        }
    }

    [BurstCompile]
    public struct ReadNativeIntArray : IJob
    {
        [ReadOnly]
        public NativeConcurrentIntArray Array;
        public int Iterations;
        public int ArrayLenght;

        public void Execute()
        {
            int value = 0;
            while (Iterations > 0)
            {
                Iterations--;
                for (int i = 0; i < ArrayLenght; i++)
                {
                    value = Array.GetValue(i);
                }
            }
            ArrayLenght = value;
        }
    }


    private NativeConcurrentIntArray _array;
    private NativeConcurrentIntArray _readArray;
    protected override void OnCreateManager()
    {
        _array = new NativeConcurrentIntArray(InitNativeCollectionsSpeedComparing.ElementsCount, Allocator.Persistent);
        _readArray = new NativeConcurrentIntArray(InitNativeCollectionsSpeedComparing.ElementsCount, Allocator.Persistent);
    }

    protected override void OnDestroyManager()
    {
        if (_array.IsCreated) _array.Dispose();
        if (_readArray.IsCreated) _readArray.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        
        var writeJob = new WriteToNativeIntArray
        {
            Array = _array,
            ArrayLenght = InitNativeCollectionsSpeedComparing.ElementsCount,
            Iterations = InitNativeCollectionsSpeedComparing.Iterations
        }.Schedule();
        writeJob.Complete(); 
        var readJob = new ReadNativeIntArray
        {
            Array = _readArray,
            ArrayLenght = InitNativeCollectionsSpeedComparing.ElementsCount,
            Iterations =  InitNativeCollectionsSpeedComparing.Iterations
        }.Schedule();
        readJob.Complete();
        //JobHandle.CompleteAll(ref writeJob, ref readJob);
        return inputDeps;
    }
}
