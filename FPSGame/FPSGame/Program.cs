using FPSGame;
using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Input;
using FPSGame.Pipelines;
using FPSGame.Scene;
using FPSGame.Texture;
using Silk.NET.Input;
using Silk.NET.Maths;
using SkiaSharp;
using System.Security.Cryptography;

Engine engine = new Engine();
List<BaseScene> scenes = new();
int currentScene = 0;

engine.OnInitialize += () =>
{
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