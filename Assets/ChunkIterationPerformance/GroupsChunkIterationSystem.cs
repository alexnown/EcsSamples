﻿using Unity.Entities;

namespace alexnown.ChunkIterationPerformance
{
    [DisableAutoCreation]
    public class GroupsChunkIterationSystem : ComponentSystem
    {
        private ComponentGroup _noTagEntities;
        private ComponentGroup _firstTagEntities;
        private ComponentGroup _secondTagEntities;

        protected override void OnCreateManager()
        {
            _noTagEntities = GetComponentGroup(ComponentType.Create<RandomValue>(),
                ComponentType.Subtractive<FirstTag>(),
                ComponentType.Subtractive<SecondTag>());
            _firstTagEntities = GetComponentGroup(ComponentType.Create<RandomValue>(), ComponentType.Create<FirstTag>());
            _secondTagEntities = GetComponentGroup(ComponentType.Create<RandomValue>(),
                ComponentType.Create<SecondTag>());
        }

        protected override void OnUpdate()
        {
            float noTagsSum = 0;
            float firstTagSum = 0;
            float secondTagSum = 0;
            if (_noTagEntities.CalculateLength() != 0)
            {
                var randArray = _noTagEntities.GetComponentDataArray<RandomValue>();
                for (int i = 0; i < randArray.Length; i++)
                {
                    noTagsSum += randArray[i].Value;
                }
            }
            if (_firstTagEntities.CalculateLength() != 0)
            {
                var randArray = _firstTagEntities.GetComponentDataArray<RandomValue>();
                for (int i = 0; i < randArray.Length; i++)
                {
                    firstTagSum += randArray[i].Value;
                }
            }
            if (_secondTagEntities.CalculateLength() != 0)
            {
                var randArray = _secondTagEntities.GetComponentDataArray<RandomValue>();
                for (int i = 0; i < randArray.Length; i++)
                {
                    secondTagSum += randArray[i].Value;
                }
            }
            if (InitializeChunkIterationWorld.LogSystemResults)
            {
                UnityEngine.Debug.Log(nameof(SingleQueryChunkIterationSystem) + $" {noTagsSum}/{firstTagSum}/{secondTagSum}");
            }
        }
    }
}
