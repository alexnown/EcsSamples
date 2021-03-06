﻿using Unity.Collections;
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
            
        }

        protected override void OnUpdate()
        {
            _firstType = GetArchetypeChunkComponentType<FirstTag>(true);
            _secondType = GetArchetypeChunkComponentType<SecondTag>(true);
            _randomType = GetArchetypeChunkComponentType<RandomValue>(true);
            var chunks = EntityManager.CreateArchetypeChunkArray(_query, Allocator.TempJob);
            double noTagsSum = 0;
            double firstTagSum = 0;
            double secondTagSum = 0;
            double totalSum = 0;
            for (int i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                var firstTag = chunk.Has(_firstType);
                var secondTag = chunk.Has(_secondType);
                var array = chunk.GetNativeArray(_randomType);
                totalSum += CalcSumValues(array);
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
            InitializeChunkIterationWorld.LogSumResults(this, noTagsSum, firstTagSum, secondTagSum, totalSum);
        }

        private double CalcSumValues(NativeArray<RandomValue> array)
        {
            double sum = 0;
            for (int i = 0; i < array.Length; i++)
            {
                var value = array[i].Value;
                sum += value;
            }
            return sum;
        }

    }
}
