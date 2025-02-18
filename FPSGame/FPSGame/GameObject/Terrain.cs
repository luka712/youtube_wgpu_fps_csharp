using BulletSharp;
using BulletSharp.Math;
using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Extensions;
using FPSGame.Pipelines;
using FPSGame.Texture;
using Silk.NET.Maths;
using SkiaSharp;

namespace FPSGame.GameObject
{
    internal unsafe class Terrain(Engine engine, DiscreteDynamicsWorld world)
    {
        UnlitRenderPipeline pipeline = null!;
        VertexBuffer vertexBuffer = new VertexBuffer(engine);
        IndexBuffer indexBuffer = new IndexBuffer(engine);
        SKImage image = SKImage.FromEncodedData("Assets/heightmap.png");
        Texture2D? texture = null;
        RigidBody rigidBody = null!;

        public void Initialize(ICamera camera)
        {
            pipeline = new UnlitRenderPipeline(engine, camera, "Unlit Render Pipeline");

            texture = new Texture2D(engine, image, "Texture2D", true);
            texture.Initialize();

            pipeline.Initialize();
            pipeline.Texture = texture;

            int terrainWidth = 64;
            int terrainLength = 64;
            float terrainScaleFactor = 16;
            Geometry terrainGeometry = GeometryBuilder.CreateTerrainGeometry(
                terrainWidth, terrainLength, terrainScaleFactor, texture);

            // VertexCount is not relevant, since we draw with indices.
            vertexBuffer.Initialize(terrainGeometry.InterleavedVertices, terrainGeometry.VertexCount);
            indexBuffer.Initialize(terrainGeometry.Indices);

            Random rand = new Random();

            // PHYSICS
            fixed (float* heightDataPtr = terrainGeometry.HeightData)
            {
                float minHeight = terrainScaleFactor * -.5f;
                float maxHeight = terrainScaleFactor * .5f;
                
                CollisionShape shape = new HeightfieldTerrainShape(
                    terrainWidth + 1,
                    terrainLength + 1,
                    (IntPtr)heightDataPtr,
                    terrainScaleFactor,
                    minHeight, maxHeight,
                    1,
                    PhyScalarType.Single,
                    false);
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
