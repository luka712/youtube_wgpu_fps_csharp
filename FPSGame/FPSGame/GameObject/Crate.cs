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
    internal class Crate(Engine engine, DiscreteDynamicsWorld world) : IDisposable
    {
        UnlitRenderPipeline pipeline = null!;
        VertexBuffer vertexBuffer = new VertexBuffer(engine);
        IndexBuffer indexBuffer = new IndexBuffer(engine);
        SKImage image = SKImage.FromEncodedData("Assets/RTS_Crate.png");
        Texture2D? texture = null;
        RigidBody rigidBody = null!;

        public float Mass { get; set; } = 1;

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
            pipeline.Transform = Matrix4X4.CreateTranslation(
                rand.NextSingle() * 10 - 5,
                rand.NextSingle() * 50 + 10,
                rand.NextSingle() * 10 - 5);

            // PHYSICS
            CollisionShape shape = new BoxShape(0.5f);
            MotionState motionState = new DefaultMotionState(pipeline.Transform.ToBulletMatrix());
            RigidBodyConstructionInfo constructionInfo = new RigidBodyConstructionInfo(Mass, motionState, shape);
            constructionInfo.LocalInertia = shape.CalculateLocalInertia(Mass);
            rigidBody = new RigidBody(constructionInfo);
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

        public void Dispose()
        {
            pipeline?.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            texture?.Dispose();
        }
    }
}
