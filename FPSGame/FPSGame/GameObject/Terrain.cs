using BulletSharp;
using BulletSharp.Math;
using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Extensions;
using FPSGame.Pipelines;
using FPSGame.Texture;
using Silk.NET.Maths;
using System.Net.Http.Headers;

namespace FPSGame.GameObject
{
    public class Terrain
    {
        // - STATIC
        private static VertexBuffer vertexBuffer = null!;
        private static IndexBuffer indexBuffer = null!;

        // - INSTANCE
        private UnlitRenderPipeline pipeline = null!;
        private RigidBody rigidBody = null!;


        public static void StaticInitialize(Engine engine)
        {
            vertexBuffer = new VertexBuffer(engine);
            indexBuffer = new IndexBuffer(engine);
        }

        public void Initialize(Engine engine, DiscreteDynamicsWorld world, ICamera camera)
        {
            pipeline = new UnlitRenderPipeline(engine, camera, "Crate Render Pipeline");

            Geometry cubeGeometry = GeometryBuilder.CreateCubeGeometry();

            // VertexCount is not relevant, since we draw with indices.
            vertexBuffer.Initialize(cubeGeometry.InterleavedVertices, cubeGeometry.VertexCount);
            indexBuffer.Initialize(cubeGeometry.Indices);

            pipeline.Initialize();

            // Add crate to physics world
            CollisionShape shape = new BoxShape(5, 0.5f, 5);
            RigidBodyConstructionInfo info = new RigidBodyConstructionInfo(0, new DefaultMotionState(), shape);
            rigidBody = new RigidBody(info);
            world.AddRigidBody(rigidBody);
            pipeline.Transform = Matrix4X4.CreateScale(10.0f, 1f, 10.0f) * Matrix4X4.CreateTranslation(0.0f, 0.0f, 0.0f);
        }

        public void Update()
        {
        }

        public void Render()
        {
            pipeline.Render(vertexBuffer, indexBuffer);
        }
    }
}
