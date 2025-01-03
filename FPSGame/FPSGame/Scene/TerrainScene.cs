using BulletSharp;
using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.GameObject;
using FPSGame.Pipelines;
using FPSGame.Texture;
using Silk.NET.Maths;
using SkiaSharp;

namespace FPSGame.Scene
{
    public class TerrainScene(Engine engine, DiscreteDynamicsWorld world) : BaseScene
    {
        Skybox skybox = new(engine);
        Terrain terrain = new(engine, world);
        List<Crate> crates = new();
        FPSCamera camera = null!;

        public override void Initialize()
        {
            camera = new FPSCamera(engine);
            camera.Position = new(0, 0, -3);
            camera.AspectRatio = engine.Window.Size.X / (float)engine.Window.Size.Y;

            skybox.Initialize(camera);
            terrain.Initialize(camera);

            for (int i = 0; i < 100; i++)
            {
                Crate crate = new Crate(engine, world);
                crates.Add(crate);
                crate.Initialize(camera);
            }
        }

        public override void Update()
        {
            camera.Update();
            foreach (Crate crate in crates)
            {
                crate.Update();
            }
        }

        public override void Render()
        {
            unsafe
            {
                engine.WGPU.RenderPassEncoderPushDebugGroup(engine.CurrentRenderPassEncoder, "Terrain Scene");
                terrain.Render();
                foreach (Crate crate in crates)
                {
                    crate.Render();
                }
                skybox.Render();
                engine.WGPU.RenderPassEncoderPopDebugGroup(engine.CurrentRenderPassEncoder);
            }
        }

        public override void Dispose()
        {

        }
    }
}
