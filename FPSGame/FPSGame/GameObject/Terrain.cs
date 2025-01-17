using BulletSharp;
using BulletSharp.Math;
using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Extensions;
using FPSGame.Pipelines;
using FPSGame.Texture;
using Silk.NET.Maths;
using SkiaSharp;
using System.Net.Http.Headers;

namespace FPSGame.GameObject
{
    internal unsafe class Terrain(Engine engine, DiscreteDynamicsWorld world)
    {
        UnlitRenderPipeline pipeline = null!;
        VertexBuffer vertexBuffer = new VertexBuffer(engine);
        IndexBuffer indexBuffer = new IndexBuffer(engine);
        SKImage image = SKImage.FromEncodedData("Assets/RTS_Crate.png");
        Texture2D? texture = null;
        RigidBody rigidBody = null!;

        public void Initialize(ICamera camera)
        {
            pipeline = new UnlitRenderPipeline(engine, camera, "Unlit Render Pipeline");

            texture = new Texture2D(engine, image, "Texture2D");
            texture.Initialize();

            pipeline.Initialize();
            pipeline.Texture = texture;

            int terrainWidth = 64;
            int terrainLength = 64;
            float terrainHeightScale = 0.5f;
            Geometry terrainGeometry = GeometryBuilder.CreateTerrainGeometry(terrainWidth, terrainLength, terrainHeightScale);

            // VertexCount is not relevant, since we draw with indices.
            vertexBuffer.Initialize(terrainGeometry.InterleavedVertices, terrainGeometry.VertexCount);
            indexBuffer.Initialize(terrainGeometry.Indices);

            Random rand = new Random();

            // PHYSICS

            fixed (float* heightDataPtr = terrainGeometry.HeightData)
            {
                float min = -0.5f * terrainHeightScale;
                float max = 0.5f * terrainHeightScale;
                HeightfieldTerrainShape shape = new HeightfieldTerrainShape(
                    terrainWidth + 1, terrainLength + 1,
                    (IntPtr) heightDataPtr,
                    terrainHeightScale, 
                    min, max,
                    1, 
                    PhyScalarType.Single,
                    false);
                shape.LocalScaling = new Vector3(1, 1, 1);
                MotionState motionState = new DefaultMotionState();
                RigidBodyConstructionInfo constructionInfo = new RigidBodyConstructionInfo(0, motionState, shape);
                rigidBody = new RigidBody(constructionInfo);
                world.AddRigidBody(rigidBody);
            }
        }

        public void Render()
        {
            pipeline.Render(vertexBuffer, indexBuffer);
        }

        public void Dispose()
        {
            pipeline?.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            texture?.Dispose();
        }
    }
}
