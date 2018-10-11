using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
        public Text DebugText;
        public int ChunksCount = 10000;
        [Range(1, 1000000)]
        public int EntitiesInChunk = 1;

        public bool LogResults;

        private double _totalSym;
        private World _world;
        private static InitializeChunkIterationWorld _instance;

        private void Start()
        {
            World.DisposeAllWorlds();
            _instance = this;
            _world = new World("ChunksIteration");

            ////_world.CreateManager<SingleGroupIterationSystem>(); //extremely slow
            //_world.CreateManager<MultyGroupsIterationSystem>();

            ////approximately equal
            //_world.CreateManager<SingleQueryChunkIterationSystem>();
            //_world.CreateManager<MultyQueryChunkIterationsSystem>();

            //_world.CreateManager<GroupsIterationBathingJobSystem>();
            _world.CreateManager<ChunkParallelIterationJobSystem>();
            _world.CreateManager<ChunkIterationBatchingJobSystem>();
            //_world.CreateManager<GroupsIterationSingleJobSystem>();

            var em = _world.GetOrCreateManager<EntityManager>();
            int elementsForType = ChunksCount / 4;
            var start = DateTime.Now;
            var startInitMsg = $"Begin initializing {ChunksCount}x{EntitiesInChunk} entities " + start;
            Debug.Log(startInitMsg);
            if (DebugText != null) DebugText.text += startInitMsg + "\n";

            InitEntities(em, elementsForType, ComponentType.Create<FirstTag>());
            InitEntities(em, elementsForType, ComponentType.Create<SecondTag>());
            InitEntities(em, elementsForType, ComponentType.Create<FirstTag>(), ComponentType.Create<SecondTag>());
            InitEntities(em, ChunksCount - 3 * elementsForType);
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.AllWorlds.ToArray());
            var msg = $"Initialize {ChunksCount * EntitiesInChunk} entities with random components total sum = {_totalSym:F2} for {DateTime.Now.Subtract(start).TotalSeconds:F2}s";
            Debug.Log(msg);
            if (DebugText != null) DebugText.text += msg;
        }

        public static void LogSumResults(ComponentSystemBase system, double noTag, double firstTag, double secondTag, double total)
        {
            if (!_instance.LogResults) return;
            UnityEngine.Debug.Log(system.GetType().Name +
                $" {noTag:F2} / {firstTag:F2} / {secondTag:F2} total={total:F2}");
        }

        public static void LogChunksCount(ComponentSystemBase system, int chunks)
        {
            if (!_instance.LogResults) return;
            UnityEngine.Debug.Log(system.GetType().Name +" chunks = "+chunks);
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
                    _totalSym += randValue;
                    em.SetComponentData(copy, new RandomValue { Value = randValue });
                }
                var randValueForInstance = Random.value;
                _totalSym += randValueForInstance;
                em.SetComponentData(instance, new RandomValue { Value = randValueForInstance });
            }
        }
    }
}
