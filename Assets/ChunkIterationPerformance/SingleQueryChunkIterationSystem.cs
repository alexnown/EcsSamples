using Unity.Collections;
using Unity.Entities;

namespace alexnown.ChunkIterationPerformance
{
    [DisableAutoCreation]
    public class SingleQueryChunkIterationSystem : ComponentSystem
    {
        private EntityArchetypeQuery _query;
        private ArchetypeChunkComponentType<FirstTag> _firstType;
        private ArchetypeChunkComponentType<SecondTag> _secondType;
        private ArchetypeChunkComponentType<RandomValue> _randomType;

        protected override void OnCreateManager()
        {
            _query = new EntityArchetypeQuery
            {
                Any = new ComponentType[0],
                All = new[] { ComponentType.Create<RandomValue>() },
                None = new ComponentType[0]
            };
            _firstType = GetArchetypeChunkComponentType<FirstTag>(true);
            _secondType = GetArchetypeChunkComponentType<SecondTag>(true);
        }

        protected override void OnUpdate()
        {
            var chunks = EntityManager.CreateArchetypeChunkArray(_query, Allocator.Persistent);
            _randomType = GetArchetypeChunkComponentType<RandomValue>(true);

            double noTagsSum = 0;
            double firstTagSum = 0;
            double secondTagSum = 0;
            for (int i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                var firstTag = chunk.Has(_firstType);
                var secondTag = chunk.Has(_secondType);
                var array = chunk.GetNativeArray(_randomType);

                if (firstTag)
                {
                    firstTagSum += CalcSumValues(array);
                }
                if (secondTag)
                {
                    secondTagSum += CalcSumValues(array);
                }
                else if (!firstTag)
                {
                    noTagsSum += CalcSumValues(array);
                }
            }
            chunks.Dispose();
            if (InitializeChunkIterationWorld.LogSystemResults)
            {
                UnityEngine.Debug.Log(nameof(SingleQueryChunkIterationSystem) + $" {noTagsSum:F3}/{firstTagSum:F3}/{secondTagSum:F3}");
            }
        }

        private double CalcSumValues(NativeArray<RandomValue> array)
        {
            double sum = 0;
            for (int i = 0; i < array.Length; i++)
            {
                sum += array[i].Value;
            }
            return sum;
        }

    }
}
