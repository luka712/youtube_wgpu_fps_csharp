using BulletSharp;
using FPSGame;
using FPSGame.Debug;
using FPSGame.Input;
using FPSGame.Scene;
using Silk.NET.Input;

Engine engine = new Engine();

DiscreteDynamicsWorld world = new DiscreteDynamicsWorld(
    new CollisionDispatcher(new DefaultCollisionConfiguration()), 
    new DbvtBroadphase(),
    new SequentialImpulseConstraintSolver(),
    new DefaultCollisionConfiguration());

List<BaseScene> scenes = new();
int currentScene = 0;

engine.OnInitialize += () =>
{
    scenes.Add(new TerrainScene(engine, world));
    scenes.Add(new SkyboxTestScene(engine));
    scenes.Add(new CubeTestScene(engine));
    scenes.Add(new QuadTestScene(engine));
    scenes[currentScene].Initialize();
};
engine.OnUpdate += () =>
{
    KeyboardState keyboardState = engine.Input.GetKeyboardState();

    if (keyboardState.IsKeyReleased(Key.Space))
    {
        scenes[currentScene].Dispose();
        currentScene++;
        if (currentScene > scenes.Count - 1)
        {
            currentScene = 0;
        }
        scenes[currentScene].Initialize();
    }

    world.StepSimulation(1f / 60f);

};
engine.OnUpdate += () =>
{
    scenes[currentScene].Update();
};
engine.OnRender += () =>
{
    scenes[currentScene].Render();
};
engine.OnDispose += () =>
{
    scenes[currentScene].Dispose();
};

engine.Initialize();
engine.Dispose();