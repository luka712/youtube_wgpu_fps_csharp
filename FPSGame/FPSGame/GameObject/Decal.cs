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

        Matrix4X4<float>[] transformsData = new Matrix4X4<float>[100];
        InstanceBuffer<Matrix4X4<float>> transformsBuffer;

        public float Mass { get; set; } = 1;

        public void Initialize(ICamera camera, Terrain terrain)
        {
            Random rand = new Random();
            for (int i = 0; i < 100; i++)
            {
                float x = rand.NextSingle() * terrain.TerrainWidth;
                float z = rand.NextSingle() * terrain.TerrainLength;

                int index = (int)z * (terrain.TerrainWidth + 1) + (int)x + 1;
                float height = terrain.HeightData[index] + 1.75f;

                x -= 32;
                z -= 32;

                transformsData[i] = Matrix4X4.CreateScale(4f, 4f, 1f)
                    * Matrix4X4.CreateTranslation(x, height, z);
            }

            transformsBuffer = new InstanceBuffer<Matrix4X4<float>>(engine, "Transforms Buffer");
            transformsBuffer.Initialize(transformsData);

            pipeline = new BillboardRenderPipeline(engine, transformsBuffer, camera, "Decal Render Pipeline");

            texture = new Texture2D(engine, image, "Texture2D");
            texture.Initialize();

            pipeline.Initialize();
            pipeline.Texture = texture;

            Geometry quadGeometry = GeometryBuilder.CreateQuadGeometry();

            // VertexCount is not relevant, since we draw with indices.
            vertexBuffer.Initialize(quadGeometry.InterleavedVertices, quadGeometry.VertexCount);
            indexBuffer.Initialize(quadGeometry.Indices);


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
