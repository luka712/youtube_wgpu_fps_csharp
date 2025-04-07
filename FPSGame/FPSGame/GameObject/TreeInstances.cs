using BulletSharp;
using BulletSharp.Math;
using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Extensions;
using FPSGame.Pipelines;
using FPSGame.Texture;
using Silk.NET.Maths;
using SkiaSharp;
using WebGPU_FPS_Game.GameObjects;

namespace FPSGame.GameObject
{
    internal class TreeInstances(Engine engine) : IDisposable
    {
        BillboardRenderPipeline pipeline = null!;
        VertexBuffer vertexBuffer = new VertexBuffer(engine);
        IndexBuffer indexBuffer = new IndexBuffer(engine);
        SKImage image = SKImage.FromEncodedData("Assets/Tree01.png");
        Texture2D? texture = null;

        public float Mass { get; set; } = 1;

        private float GetHeight(Terrain terrain, int x, int z)
        {
            int index = x + z * terrain.TerrainWidth;
            return terrain.HeightData[index] * terrain.TerrainHeightFactor;
        }

        public void Initialize(ICamera camera, Terrain terrain)
        {
            pipeline = new BillboardRenderPipeline(engine, camera, "Tree Render Pipeline");

            texture = new Texture2D(engine, image, "Texture2D");
            texture.Initialize();

            pipeline.Initialize();
            pipeline.Texture = texture;

            Geometry quadGeometry = GeometryBuilder.CreateQuadGeometry();

            // VertexCount is not relevant, since we draw with indices.
            vertexBuffer.Initialize(quadGeometry.InterleavedVertices, quadGeometry.VertexCount);
            indexBuffer.Initialize(quadGeometry.Indices);

            float height = GetHeight(terrain, terrain.TerrainWidth / 2, terrain.TerrainLength / 2) * 2;

            pipeline.Transform = Matrix4X4.CreateScale(4f, 4, 4) * Matrix4X4.CreateTranslation(
                0.0f,
                height + 2,
                0);
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
