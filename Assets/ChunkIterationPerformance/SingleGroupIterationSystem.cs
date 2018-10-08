using Unity.Entities;

namespace alexnown.ChunkIterationPerformance
{
    [DisableAutoCreation]
    public class SingleGroupIterationSystem : ComponentSystem
    {
        private ComponentGroup _group;
        [Inject]
        private ComponentDataFromEntity<FirstTag> _entitiesWithFirstTag;
        [Inject]
        private ComponentDataFromEntity<SecondTag> _entitiesWithSecondTag;

        protected override void OnCreateManager()
        {
            _group = GetComponentGroup(ComponentType.Create<RandomValue>());
        }

        protected override void OnUpdate()
        {
            double noTagsSum = 0;
            double firstTagSum = 0;
            double secondTagSum = 0;
            double totalSum = 0;
            var randomValues = _group.GetComponentDataArray<RandomValue>();
            var entities = _group.GetEntityArray();
            for (int i = 0; i < randomValues.Length; i++)
            {
                var randomValue = randomValues[i].Value;
                var entity = entities[i];
                bool hasFirst = _entitiesWithFirstTag.Exists(entity);
                bool hasSecond = _entitiesWithSecondTag.Exists(entity);
                totalSum += randomValue;
                if (hasFirst) firstTagSum += randomValue;
                if (hasSecond) secondTagSum += randomValue;
                else if (!hasFirst) noTagsSum += randomValue;
            }
            InitializeChunkIterationWorld.LogSumResults(this, noTagsSum, firstTagSum, secondTagSum, totalSum);
        }
    }
}
