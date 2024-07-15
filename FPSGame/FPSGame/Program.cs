using FPSGame;
using FPSGame.Buffers;
using FPSGame.Pipelines;

Engine engine = new Engine();

UnlitRenderPipeline unlitRenderPipeline = new UnlitRenderPipeline(engine);
VertexBuffer vertexBuffer = new VertexBuffer(engine);
IndexBuffer indexBuffer = new IndexBuffer(engine);

engine.OnInitialize += () =>
{
    unlitRenderPipeline.Initialize();
    vertexBuffer.Initialize(new float[]
        {
            // x,y,z            r,g,b,a
            -0.5f, -0.5f, 0f,    1f, 0f, 0f, 1f,
            0.5f, -0.5f, 0f,     0f, 1f, 0f, 1f,
            -0.5f, 0.5f, 0f,     0f, 0f, 1f, 1f,
            0.5f, 0.5f, 0f,      1f, 0f, 0f, 1f,
        }, 4);
    
     indexBuffer.Initialize(new ushort[]
     {
         0,1,2,
         1,3,2
     });
};

engine.OnRender += () =>
{
    unlitRenderPipeline.Render(vertexBuffer, indexBuffer);
};

engine.OnDispose += () =>
{
    unlitRenderPipeline.Dispose();
    vertexBuffer.Dispose();
    indexBuffer.Dispose();
};

engine.Initialize();
engine.Dispose();
