using FPSGame;
using FPSGame.Buffers;
using FPSGame.Pipelines;
using FPSGame.Texture;
using Silk.NET.Maths;
using SkiaSharp;

Engine engine = new Engine();

UnlitRenderPipeline unlitRenderPipeline = new UnlitRenderPipeline(engine);
VertexBuffer vertexBuffer = new VertexBuffer(engine);
IndexBuffer indexBuffer = new IndexBuffer(engine);
SKImage image = SKImage.FromEncodedData("Assets/test.png");
Texture2D? texture = null;

float scl = 1;
int sclDir = 1;

engine.OnInitialize += () =>
{
    if (image is null)
    {
        throw new FileNotFoundException("Unable to load image.");
    }

    texture = new Texture2D(engine, image, "Texture2D");
    texture.Initialize();
    
    unlitRenderPipeline.Initialize();
    unlitRenderPipeline.Texture = texture;
    vertexBuffer.Initialize(new float[]
    {
        -0.5f, -0.5f, 0f,    1, 0, 0, 1,  0,1, // v0
        0.5f, -0.5f, 0f,    0, 1, 0, 1,   1,1,// v1
        -0.5f,  0.5f, 0f,    0, 0, 1, 1,  0,0,// v2
        0.5f,  0.5f, 0f,    0, 1, 0, 1,    1,0// v3
    }, 6);

    indexBuffer.Initialize(new ushort[]
    {
        0,1,2, // t0
        1,3,2  // t1
    });
};
engine.OnRender += () =>
{
    if(scl > 2)
    {
        sclDir = -1;
    }
    else if(scl < 0.5)
    {
        sclDir = 1;
    }
    scl += 0.01f * sclDir;

    unlitRenderPipeline.Transform = Matrix4X4.CreateScale<float>(scl, scl, 1.0f);

    unlitRenderPipeline.Render(vertexBuffer, indexBuffer);
};
engine.OnDispose += () =>
{
    unlitRenderPipeline?.Dispose();
    vertexBuffer.Dispose();
    indexBuffer.Dispose();
    texture?.Dispose();
};

engine.Initialize();
engine.Dispose();