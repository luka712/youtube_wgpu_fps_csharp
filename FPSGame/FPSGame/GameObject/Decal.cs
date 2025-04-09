using FPSGame.Buffers;
using FPSGame.Texture;
using SkiaSharp;
using FPSGame;
using Silk.NET.Maths;
using WebGPU_FPS_Game.Pipelines;
using FPSGame.Camera;

namespace WebGPU_FPS_Game.GameObjects
{
    internal class Decal(Engine engine) : IDisposable
    {
        BillboardRenderPipeline pipeline = null!;
        VertexBuffer vertexBuffer = new VertexBuffer(engine);
        IndexBuffer indexBuffer = new IndexBuffer(engine);
        SKImage image = SKImage.FromEncodedData("Assets/Tree01.png");
        Texture2D? texture = null;

        public float Mass { get; set; } = 1;

        public void Initialize(ICamera camera)
        {
            pipeline = new BillboardRenderPipeline(engine, camera, "Decal Render Pipeline");

            texture = new Texture2D(engine, image, "Texture2D");
            texture.Initialize();

            pipeline.Initialize();
            pipeline.Texture = texture;

            Geometry quadGeometry = GeometryBuilder.CreateQuadGeometry();

            // VertexCount is not relevant, since we draw with indices.
            vertexBuffer.Initialize(quadGeometry.InterleavedVertices, quadGeometry.VertexCount);
            indexBuffer.Initialize(quadGeometry.Indices);

            pipeline.Transform = Matrix4X4.CreateScale(4f, 4f, 1f) * Matrix4X4.CreateTranslation(
                0f,
                4,
                0f);
        }

        public void Update()
        {
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
