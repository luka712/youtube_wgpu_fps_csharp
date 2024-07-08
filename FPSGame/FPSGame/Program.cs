using FPSGame;
using FPSGame.Buffers;
using FPSGame.Pipelines;

Engine engine = new Engine();

UnlitRenderPipeline unlitRenderPipeline = new UnlitRenderPipeline(engine);
VertexBuffer vertexBuffer = new VertexBuffer(engine);

engine.OnInitialize += () =>
{
    unlitRenderPipeline.Initialize();
    vertexBuffer.Initialize(new float[]
        {
            // v1 
            -0.5f, -0.5f, 0f,    1f, 0f, 0f, 1f,
            0.5f, -0.5f, 0f,     0f, 1f, 0f, 1f,
            0f, 0.5f, 0f,        0f, 0f, 1f, 1f
        });
};

engine.OnRender += () =>
{
    unlitRenderPipeline.Render(vertexBuffer);
};

engine.OnDispose += () =>
{
    unlitRenderPipeline.Dispose();
    vertexBuffer.Dispose();
};

engine.Initialize();
engine.Dispose();
