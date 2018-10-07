using Unity.Entities;

namespace alexnown.ChunkIterationPerformance
{
    [DisableAutoCreation]
    public class GroupsChunkIterationSystem : ComponentSystem
    {
        private ComponentGroup _noTagEntities;
        private ComponentGroup _firstTagEntities;
        private ComponentGroup _secondTagEntities;
        private ComponentGroup _allRandomEntities;

        protected override void OnCreateManager()
        {
            _noTagEntities = GetComponentGroup(ComponentType.Create<RandomValue>(),
                ComponentType.Subtractive<FirstTag>(),
                ComponentType.Subtractive<SecondTag>());
            _firstTagEntities = GetComponentGroup(ComponentType.Create<RandomValue>(), ComponentType.Create<FirstTag>());
            _secondTagEntities = GetComponentGroup(ComponentType.Create<RandomValue>(),
                ComponentType.Create<SecondTag>());
            _allRandomEntities = GetComponentGroup(ComponentType.Create<RandomValue>());
        }

        protected override void OnUpdate()
        {
            double noTagsSum = 0;
            double firstTagSum = 0;
            double secondTagSum = 0;
            double totalSum = 0;
            if (_noTagEntities.CalculateLength() != 0)
            {
                var randArray = _noTagEntities.GetComponentDataArray<RandomValue>();
                for (int i = 0; i < randArray.Length; i++)
                {
                    var value = randArray[i].Value;
                    noTagsSum += value;
                }
            }
            if (_firstTagEntities.CalculateLength() != 0)
            {
                var randArray = _firstTagEntities.GetComponentDataArray<RandomValue>();
                for (int i = 0; i < randArray.Length; i++)
                {
                    var value = randArray[i].Value;
                    firstTagSum += value;
                }
            }
            if (_secondTagEntities.CalculateLength() != 0)
            {
                var randArray = _secondTagEntities.GetComponentDataArray<RandomValue>();
                for (int i = 0; i < randArray.Length; i++)
                {
                    var value = randArray[i].Value;
                    secondTagSum += value;
                }
            }
            var allRandArray = _allRandomEntities.GetComponentDataArray<RandomValue>();
            for (int i = 0; i < allRandArray.Length; i++)
            {
                var value = allRandArray[i].Value;
                totalSum += value;
            }

            InitializeChunkIterationWorld.LogSumResults(this, noTagsSum, firstTagSum, secondTagSum, totalSum);
        }
    }
}
