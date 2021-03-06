﻿using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

[StructLayout(LayoutKind.Sequential)]
[NativeContainer]
unsafe public struct NativeByteArray
{

    public int Length { get; private set; }
    // The actual pointer to the allocated count needs to have restrictions relaxed so jobs can be schedled with this container
    [NativeDisableUnsafePtrRestriction]
    byte* m_Counter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
    AtomicSafetyHandle m_Safety;
    // The dispose sentinel tracks memory leaks. It is a managed type so it is cleared to null when scheduling a job
    // The job cannot dispose the container, and no one else can dispose it until the job has run so it is ok to not pass it along
    // This attribute is required, without it this native container cannot be passed to a job since that would give the job access to a managed object
    [NativeSetClassTypeToNullOnSchedule]
    DisposeSentinel m_DisposeSentinel;
#endif

    // Keep track of where the memory for this was allocated
    Allocator m_AllocatorLabel;

    public NativeByteArray(int size, Allocator label)
    {
        Length = size;
        // This check is redundant since we always use an int which is blittable.
        // It is here as an example of how to check for type correctness for generic types.
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (!UnsafeUtility.IsBlittable<byte>())
            throw new ArgumentException(string.Format("{0} used in NativeQueue<{0}> must be blittable", typeof(int)));
#endif
        m_AllocatorLabel = label;

        // Allocate native memory for a single integer
        m_Counter = (byte*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<byte>() * size, UnsafeUtility.AlignOf<byte>(), label);

        // Create a dispose sentinel to track memory leaks. This also creates the AtomicSafetyHandle
#if ENABLE_UNITY_COLLECTIONS_CHECKS
#if UNITY_2018_3_OR_NEWER
        DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, label);
#else
        DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0);
#endif
#endif
        // Initialize the count to 0 to avoid uninitialized data
        for (int i = 0; i < size; ++i)
        {
            *(m_Counter + i) = 0;
        }
    }

    public byte this[int index]
    {
        get
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            return *(m_Counter + index);
        }
        
        set
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            *(m_Counter + index) = value;
        }
    }
    
    public bool IsCreated
    {
        get { return m_Counter != null; }
    }

    public void Dispose()
    {
        // Let the dispose sentinel know that the data has been freed so it does not report any memory leaks
#if ENABLE_UNITY_COLLECTIONS_CHECKS
#if UNITY_2018_3_OR_NEWER
        DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#else
        DisposeSentinel.Dispose(m_Safety, ref m_DisposeSentinel);
#endif
#endif

        UnsafeUtility.Free(m_Counter, m_AllocatorLabel);
        m_Counter = null;
    }

    /*
    public Concurrent ToConcurrent()
    {
        Concurrent concurrent;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
        concurrent.m_Safety = m_Safety;
        AtomicSafetyHandle.UseSecondaryVersion(ref concurrent.m_Safety);
#endif

        concurrent.m_Counter = m_Counter;
        return concurrent;
    }
    
    [NativeContainer]
    // This attribute is what makes it possible to use NativeCounter.Concurrent in a ParallelFor job
    [NativeContainerIsAtomicWriteOnly]
    unsafe public struct Concurrent
    {
        // Copy of the pointer from the full NativeCounter
        [NativeDisableUnsafePtrRestriction]
        internal int* m_Counter;

        // Copy of the AtomicSafetyHandle from the full NativeCounter. The dispose sentinel is not copied since this inner struct does not own the memory and is not responsible for freeing it
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;
#endif

        public void Increment(int index)
        {
            // Increment still needs to check for write permissions
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            // The actual increment is implemented with an atomic since it can be incremented by multiple threads at the same time
            Interlocked.Increment(ref *(m_Counter + index));
        }

        public void Add(int index, int value)
        {
            // Increment still needs to check for write permissions
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            // The actual increment is implemented with an atomic since it can be incremented by multiple threads at the same time
            Interlocked.Add(ref *(m_Counter + index), value);
        }
    } */
}
