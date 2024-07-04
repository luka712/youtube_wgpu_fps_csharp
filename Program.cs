using FPSGame;
using FPSGame.Pipelines;

Engine engine = new Engine();

UnlitRenderPipeline pipeline = new UnlitRenderPipeline(engine);

engine.OnInitialize += () =>
{
    pipeline.Initialize();
};

engine.OnDraw += () =>
{
    pipeline.Render();
};

engine.Initialize();
engine.Dispose();