using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
namespace alexnown.ChunkIterationPerformance
{
    public struct SharedComponent : ISharedComponentData
    {
        public int Value;
    }

    public struct FirstTag : IComponentData { }
    public struct SecondTag : IComponentData { }

    public struct RandomValue : IComponentData
    {
        public double Value;
    }


    public class InitializeChunkIterationWorld : MonoBehaviour
    {
        public int ChunksCount = 10000;
        [Range(1, 1000000)]
        public int EntitiesInChunk = 1;

        public bool LogResults;

        private double _totalRandomComponentsSym;
        private World _world;
        private static InitializeChunkIterationWorld _instance;
        private void Start()
        {
            _instance = this;
            _world = new World("ChunksIteration");
            _world.CreateManager<SingleQueryChunkIterationSystem>();
            _world.CreateManager<GroupsChunkIterationSystem>();
            _world.CreateManager<GroupsIterationBathingJobSystem>();
            _world.CreateManager<GroupsIterationSingleJobSystem>();
            var em = _world.GetOrCreateManager<EntityManager>();
            int elementsForType = ChunksCount / 4;
            InitEntities(em, elementsForType, ComponentType.Create<FirstTag>());
            InitEntities(em, elementsForType, ComponentType.Create<SecondTag>());
            InitEntities(em, elementsForType, ComponentType.Create<FirstTag>(), ComponentType.Create<SecondTag>());
            InitEntities(em, ChunksCount - 3 * elementsForType);
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.AllWorlds.ToArray());
            Debug.Log($"Initialize {ChunksCount * EntitiesInChunk} entities with random components total sum = {_totalRandomComponentsSym:F2}");
        }

        public static void LogSumResults(ComponentSystemBase system, double noTag, double firstTag, double secondTag, double total)
        {
            if (!_instance.LogResults) return;
            UnityEngine.Debug.Log(system.GetType().Name +
                $" {noTag:F2} / {firstTag:F2} / {secondTag:F2} total={total:F2}");
        }

        private void InitEntities(EntityManager em, int count, params ComponentType[] components)
        {
            var componentsList = new List<ComponentType>() { ComponentType.Create<SharedComponent>(), ComponentType.Create<RandomValue>() };
            componentsList.AddRange(components);
            var archetype = em.CreateArchetype(componentsList.ToArray());
            for (int i = 0; i < count; i++)
            {
                var shared = new SharedComponent { Value = i };
                var instance = em.CreateEntity(archetype);
                em.SetSharedComponentData(instance, shared);
                for (int j = 0; j < EntitiesInChunk - 1; j++)
                {
                    var copy = em.Instantiate(instance);
                    var randValue = Random.value;
                    _totalRandomComponentsSym += randValue;
                    em.SetComponentData(copy, new RandomValue { Value = randValue });
                }
                var randValueForInstance = Random.value;
                _totalRandomComponentsSym += randValueForInstance;
                em.SetComponentData(instance, new RandomValue { Value = randValueForInstance });
            }
        }

        private void OnDestroy()
        {
            if (!_world.IsCreated)
            {
                _world.Dispose();
                ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.AllWorlds.ToArray());
            }
        }
    }
}
