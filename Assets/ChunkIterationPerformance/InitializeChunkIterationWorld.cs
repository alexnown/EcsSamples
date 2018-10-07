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
        public float Value;
    }


    public class InitializeChunkIterationWorld : MonoBehaviour
    {
        public int ChunksCount = 10000;
        [Range(1, 1000000)]
        public int EntitiesInChunk = 1;

        private World _world;
        private void Start()
        {
            _world = new World("ChunksIteration");
            _world.CreateManager<SingleQueryChunkIteration>();
            var em = _world.GetOrCreateManager<EntityManager>();
            int elementsForType = ChunksCount / 3;
            InitEntities(em, elementsForType, ComponentType.Create<FirstTag>());
            InitEntities(em, elementsForType, ComponentType.Create<SecondTag>());
            InitEntities(em, ChunksCount - 2 * elementsForType);
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.AllWorlds.ToArray());
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
                    em.SetComponentData(copy, new RandomValue { Value = Random.value });
                }
                em.SetComponentData(instance, new RandomValue { Value = Random.value });
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
