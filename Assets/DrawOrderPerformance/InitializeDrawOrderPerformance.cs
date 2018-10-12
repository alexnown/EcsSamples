using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

namespace alexnown.DrawOrderPerformance
{

    public class InitializeDrawOrderPerformance : MonoBehaviour
    {
        public Material DrawMaterial;
        public SpriteAtlas Atlas;
        [Range(1, 1000)]
        public int Multiplier = 1;
        public int MovePath = 10;

        private void Start()
        {
            World.DisposeAllWorlds();
            var world = new World("DrawOrderPerformance");
            world.CreateManager<SharedSpriteRendererSystem>();

            var sprites = new Sprite[Atlas.spriteCount];
            Atlas.GetSprites(sprites);
            CreateEntitiesFromSprites(world, sprites);

            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.AllWorlds.ToArray());
        }

        private void CreateEntitiesFromSprites(World world, Sprite[] sprites)
        {
            if (sprites == null || sprites.Length == 0 || Multiplier < 1) return;
            var em = world.GetOrCreateManager<EntityManager>();
            var archetype = em.CreateArchetype(ComponentType.Create<Position>(),
                ComponentType.Create<SharedSprite>());//, ComponentType.Create<MeshInstanceRenderer>());
            foreach (var sprite in sprites)
            {
                var material = Instantiate(DrawMaterial);
                material.enableInstancing = true;
                material.mainTexture = sprite.texture;
                var mesh = SharedSpriteRendererSystem.CreateMeshForSprite(sprite);
                /*var meshData = new MeshInstanceRenderer
                {
                    mesh = mesh,
                    material = material,
                    castShadows = ShadowCastingMode.Off
                }; */
                var charedData = new SharedSprite
                {
                    mat = material,
                    mesh = mesh
                };
                var firstEntity = em.CreateEntity(archetype);
                //em.SetSharedComponentData(firstEntity, meshData);
                em.SetSharedComponentData(firstEntity, charedData);
                if (Multiplier > 1)
                {
                    for (int i = 0; i < Multiplier - 1; i++)
                    {
                        var copy = em.Instantiate(firstEntity);
                        em.SetComponentData(copy, GetRandomPosition(MovePath));
                    }
                }
                em.SetComponentData(firstEntity, GetRandomPosition(MovePath));
            }
        }

        private Position GetRandomPosition(float movePath)
        {
            float x = Random.Range(-movePath / 2, movePath / 2);
            return new Position { Value = new float3(x, Random.Range(-4, 4), 0) };
        }
    }
}
