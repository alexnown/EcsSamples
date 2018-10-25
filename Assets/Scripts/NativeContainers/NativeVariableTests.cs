using System;
using alexnown.NativeContainers;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using Assert = NUnit.Framework.Assert;

namespace alexnown.Tests
{
    [TestFixture]
    [NUnit.Framework.Category("NativeVariable")]
    public class NativeVariableTests
    {
        [Test]
        public void IntNativeVariable()
        {
            var intVariable = new NativeVariable<int>(Allocator.Persistent);
            Assert.AreEqual(0, intVariable.Value);

            int settedValue = -25;
            intVariable.Value = settedValue;
            Assert.AreEqual(settedValue, intVariable.Value);

            intVariable.Dispose();
        }

        [Test]
        public void BlittableStructNativeVariable()
        {
            var variable = new NativeVariable<BlittableStruct>(Allocator.Temp);
            Assert.AreEqual(default(BlittableStruct), variable.Value);

            var myStract = new BlittableStruct { Id = long.MaxValue, Price = 123.45f };
            variable.Value = myStract;
            Assert.AreEqual(myStract, variable.Value);

            variable.Dispose();
        }

        [Test]
        public void NotBlittableChecks()
        {
            Action initNotblittableVariable = () =>
            {
                var variable = new NativeVariable<NotBlittableStruct>(Allocator.TempJob);
                variable.Dispose();
            };
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.Throws<InvalidOperationException>(() => initNotblittableVariable.Invoke());
#else
            Assert.DoesNotThrow(()=> initNotblittableVariable.Invoke());
#endif
        }

        [Test]
        public void CheckDispose()
        {
            Assert.Throws<InvalidOperationException>(() => default(NativeVariable<int>).Dispose());
            var variable = new NativeVariable<int>(Allocator.Temp);
            Assert.False(variable.Disposed);
            variable.Dispose();
            Assert.True(variable.Disposed);
            Assert.DoesNotThrow(() => variable.Dispose());
        }

        [Test]
        public void GetSetValueAfterDispose()
        {
            var variable = new NativeVariable<int>(Allocator.Temp);
            variable.Dispose();
            Assert.Throws<InvalidOperationException>(() => variable.Value = 25);
            Assert.Throws<InvalidOperationException>(() => { int myValue = variable.Value; });
        }

        struct BlittableStruct
        {
            public long Id;
            public float Price;
        }

        struct NotBlittableStruct
        {
            public GameObject Go;
        }
    }
}