using BulletSharp;
using BulletSharp.Math;
using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Extensions;
using FPSGame.Pipelines;
using FPSGame.Texture;
using Silk.NET.Maths;

namespace FPSGame.GameObject
{
    public class Crate
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

        public void Initialize(Engine engine, DiscreteDynamicsWorld world, ICamera camera, Texture2D texture, Matrix4X4<float> transform)
        {
            pipeline = new UnlitRenderPipeline(engine, camera, "Crate Render Pipeline");

            Geometry cubeGeometry = GeometryBuilder.CreateCubeGeometry();

            // VertexCount is not relevant, since we draw with indices.
            vertexBuffer.Initialize(cubeGeometry.InterleavedVertices, cubeGeometry.VertexCount);
            indexBuffer.Initialize(cubeGeometry.Indices);

            pipeline.Initialize();
            pipeline.Texture = texture;

            // Add crate to physics world
            CollisionShape shape = new BoxShape(0.5f);
            MotionState motionState = new DefaultMotionState(transform.ToBulletMatrix());
            RigidBodyConstructionInfo info = new RigidBodyConstructionInfo(1, motionState, shape);
            info.LocalInertia = shape.CalculateLocalInertia(1);
            rigidBody = new RigidBody(info);
            world.AddRigidBody(rigidBody);
        }

        public void Update()
        {
            rigidBody.GetWorldTransform(out Matrix transform);
            pipeline.Transform = transform.ToSilkNetMatrix();
        }

        public void Render()
        {
            pipeline.Render(vertexBuffer, indexBuffer);
        }
    }
}
