using FPSGame;
using FPSGame.Pipelines;

Engine engine = new Engine();

UnlitRenderPipeline unlitRenderPipeline = new UnlitRenderPipeline(engine);

engine.OnInitialize += () =>
{
    unlitRenderPipeline.Initialize();
};
engine.OnRender += () =>
{
    unlitRenderPipeline.Render();
};

engine.Initialize();
engine.Dispose();
