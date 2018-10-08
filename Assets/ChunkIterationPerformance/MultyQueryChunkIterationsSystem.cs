using Unity.Collections;
using Unity.Entities;

namespace alexnown.ChunkIterationPerformance
{
    [DisableAutoCreation]
    public class MultyQueryChunkIterationsSystem : ComponentSystem
    {
        private EntityArchetypeQuery _allRandomQuery;
        private EntityArchetypeQuery _noTagQuery;
        private EntityArchetypeQuery _firstTagQuery;
        private EntityArchetypeQuery _secondTagQuery;
        
        private ArchetypeChunkComponentType<RandomValue> _randomType;

        protected override void OnCreateManager()
        {
            _allRandomQuery = new EntityArchetypeQuery
            {
                Any = new ComponentType[0],
                All = new[] { ComponentType.Create<RandomValue>() },
                None = new ComponentType[0]
            };
            _noTagQuery = new EntityArchetypeQuery
            {
                Any = new ComponentType[0],
                All = new[] { ComponentType.Create<RandomValue>() },
                None = new[] { ComponentType.Create<FirstTag>(), ComponentType.Create<SecondTag>() }
            };
            _firstTagQuery = new EntityArchetypeQuery
            {
                Any = new ComponentType[0],
                All = new[] { ComponentType.Create<RandomValue>(), ComponentType.Create<FirstTag>() },
                None = new ComponentType[0]
            };
            _secondTagQuery = new EntityArchetypeQuery
            {
                Any = new ComponentType[0],
                All = new[] { ComponentType.Create<RandomValue>(), ComponentType.Create<SecondTag>(), },
                None = new ComponentType[0]
            };
        }

        private double CalcSumFromChanks(EntityArchetypeQuery query)
        {
            var chunks = EntityManager.CreateArchetypeChunkArray(query, Allocator.TempJob);
            double sum = 0;
            for (int i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                var array = chunk.GetNativeArray(_randomType);
                for (int j = 0; j < array.Length; j++)
                {
                    var value = array[j].Value;
                    sum += value;
                }
            }
            chunks.Dispose();
            return sum;
        }

        protected override void OnUpdate()
        {
            _randomType = GetArchetypeChunkComponentType<RandomValue>(true);

            double noTagsSum = CalcSumFromChanks(_noTagQuery);
            double firstTagSum = CalcSumFromChanks(_firstTagQuery);
            double secondTagSum = CalcSumFromChanks(_secondTagQuery);
            double totalSum = CalcSumFromChanks(_allRandomQuery);
            InitializeChunkIterationWorld.LogSumResults(this, noTagsSum, firstTagSum, secondTagSum, totalSum);
        }
    }
}
