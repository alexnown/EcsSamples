using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace alexnown.NativeContainers
{
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    [NativeContainerSupportsDeallocateOnJobCompletion]
    unsafe public struct NativeVariable<T> where T : struct
    {
        public Allocator Allocator { get; private set; }
        public bool Disposed { get; private set; }

        internal AtomicSafetyHandle _Safety;
        [NativeSetClassTypeToNullOnSchedule]
        internal DisposeSentinel _DisposeSentinel;

        public T Value
        {
            get
            {
                CheckIsDisposedAndThrow();
                return UnsafeUtility.ReadArrayElement<T>(_addr, 0);
            }
            set
            {
                CheckIsDisposedAndThrow();
                UnsafeUtility.WriteArrayElement(_addr, 0, value);
            }
        }

        [NativeDisableUnsafePtrRestriction]
        private void* _addr;

        public NativeVariable(Allocator allocator)
        {
            
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!UnsafeUtility.IsBlittable<T>())
            {
                throw new InvalidOperationException($"{typeof(T)} used in NativeVariable<{typeof(T)}> must be blittable.");
            }
#endif
            Disposed = false;
            Allocator = allocator;
            int size = UnsafeUtility.SizeOf<T>();
            _addr = UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear(_addr, size);
            DisposeSentinel.Create(out _Safety, out _DisposeSentinel, 1, allocator);
        }

        [WriteAccessRequired]
        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!UnsafeUtility.IsValidAllocator(Allocator))
                throw new InvalidOperationException("The NativeVariable can not be Disposed because it was not allocated with a valid allocator.");
#endif
            UnsafeUtility.Free(_addr, Allocator);
            _addr = null;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void CheckIsDisposedAndThrow()
        {
            if(Disposed) throw new InvalidOperationException("NativeVariable already disposed.");
        }
        /*
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckElementReadAccess(int index)
        {
            var versionPtr = (AtomicSafetyHandleVersionMask*)_Safety.versionNode;
            if ((_Safety.version & AtomicSafetyHandleVersionMask.Read) == 0 && _Safety.version != ((*versionPtr) & AtomicSafetyHandleVersionMask.WriteInv))
                AtomicSafetyHandle.CheckReadAndThrowNoEarlyOut(_Safety);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckElementWriteAccess(int index)
        {
            if (index < m_MinIndex || index > m_MaxIndex)
                FailOutOfRangeError(index);

            var versionPtr = (AtomicSafetyHandleVersionMask*)m_Safety.versionNode;
            if ((m_Safety.version & AtomicSafetyHandleVersionMask.Write) == 0 && m_Safety.version != ((*versionPtr) & AtomicSafetyHandleVersionMask.ReadInv))
                AtomicSafetyHandle.CheckWriteAndThrowNoEarlyOut(m_Safety);
        } */

    }
}
