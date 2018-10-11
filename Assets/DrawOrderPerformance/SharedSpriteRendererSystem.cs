using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace alexnown.DrawOrderPerformance
{
    [Serializable]
    public struct SharedSprite : ISharedComponentData
    {
        public Mesh mesh;
        public Material mat;
    }
    

    [UpdateAfter(typeof(PreLateUpdate.ParticleSystemBeginUpdateAll))]
    [ExecuteInEditMode] [DisableAutoCreation]
    public class SharedSpriteRendererSystem : ComponentSystem
    {
        private List<SharedSprite> cachedUniqueSharedSprites = new List<SharedSprite>();

        private ComponentGroup componentGroup;

        protected override void OnCreateManager()
        {
            componentGroup = GetComponentGroup(typeof(SharedSprite), typeof(Position));
        }

        protected override void OnUpdate()
        {
            EntityManager.GetAllUniqueSharedComponentData(cachedUniqueSharedSprites);
            for (int i = 0; i != cachedUniqueSharedSprites.Count; i++)
            {
                var sharedSprite = cachedUniqueSharedSprites[i];
                
                componentGroup.SetFilter(sharedSprite);
                var positions = componentGroup.GetComponentDataArray<Position>();
                
                for (int j = 0; j < positions.Length; j++)
                {
                    Graphics.DrawMesh(sharedSprite.mesh, positions[j].Value, Quaternion.identity, sharedSprite.mat, 0);
                }
                
            }
            cachedUniqueSharedSprites.Clear();
        }

        public static Mesh CreateMeshForSprite(Sprite sprite)
        {
            var spriteVertices = sprite.vertices;
            Vector3[] vertices = new Vector3[spriteVertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = spriteVertices[i];
            }

            var spriteTriangles = sprite.triangles;
            var triangles = new int[spriteTriangles.Length];
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] = spriteTriangles[i];
            }

            return new Mesh
            {
                vertices = vertices,
                uv = sprite.uv,
                triangles = triangles
            };
        }

        public static Mesh generateQuad(Vector2 size, Vector2 pivot)
        {
            Vector3[] vertices = new Vector3[] {
                new Vector3(size.x - pivot.x, size.y - pivot.y),
                new Vector3(size.x - pivot.x, 0 - pivot.y),
                new Vector3(0 - pivot.x,  0 - pivot.y),
                new Vector3(0 - pivot.x,size.y - pivot.y)
            };

            Vector2[] uv = new Vector2[] {
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1)
            };

            int[] triangles = new int[] {
                0, 1, 2,
                2, 3, 0
            };

            return new Mesh
            {
                vertices = vertices,
                uv = uv,
                triangles = triangles
            };
        }
    }
}