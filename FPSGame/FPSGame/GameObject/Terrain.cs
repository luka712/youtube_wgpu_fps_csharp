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
    internal class Terrain(Engine engine, DiscreteDynamicsWorld world)
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

            Geometry cubeGeometry = GeometryBuilder.CreateCubeGeometry();

            // VertexCount is not relevant, since we draw with indices.
            vertexBuffer.Initialize(cubeGeometry.InterleavedVertices, cubeGeometry.VertexCount);
            indexBuffer.Initialize(cubeGeometry.Indices);

            Random rand = new Random();
            pipeline.Transform = Matrix4X4.CreateTranslation(rand.NextSingle() * 10 - 5, rand.NextSingle() * 10 - 5, rand.NextSingle() * 10 - 5);

            // PHYSICS
            CollisionShape shape = new BoxShape(5, 0.5f, 5);
            MotionState motionState = new DefaultMotionState();
            RigidBodyConstructionInfo constructionInfo = new RigidBodyConstructionInfo(0, motionState, shape);
            rigidBody = new RigidBody(constructionInfo);
            pipeline.Transform = Matrix4X4.CreateScale(10.0f, 1, 10);
            world.AddRigidBody(rigidBody);
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
