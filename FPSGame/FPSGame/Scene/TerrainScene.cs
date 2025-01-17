using FPSGame.Scene;
using FPSGame;
using BulletSharp;
using WebGPU_FPS_Game.DebugObjects;
using FPSGame.Camera;
using FPSGame.GameObject;

namespace WebGPU_FPS_Game.Scene
{
    public class TerrainScene(Engine engine, DiscreteDynamicsWorld world) : BaseScene
    {
        const bool DEBUG = true;

        Skybox skybox = new(engine);
        Terrain terrain = new(engine, world);
        List<Crate> crates = new();
        FPSCamera camera = null!;
        BulletDebugDrawable bulletDebugDrawable = null!;


        public override void Initialize()
        {
            camera = new FPSCamera(engine);
            camera.Position = new(0, 0, -3);
            camera.AspectRatio = engine.Window.Size.X / (float)engine.Window.Size.Y;

            skybox.Initialize(camera);
            terrain.Initialize(camera);

            for (int i = 0; i < 20; i++)
            {
                Crate crate = new Crate(engine, world);
                crates.Add(crate);
                crate.Initialize(camera);
            }

            bulletDebugDrawable = new BulletDebugDrawable(engine, camera);
            bulletDebugDrawable.Initialize();
            world.DebugDrawer = bulletDebugDrawable;
        }

        public override void Update()
        {
            camera.Update();
            foreach (Crate crate in crates)
            {
                crate.Update();
            }

            if (DEBUG)
            {
                world.DebugDrawWorld();
            }
        }

        public override void Render()
        {
            unsafe
            {
                if (DEBUG)
                {
                    bulletDebugDrawable.Render();
                }

                engine.WGPU.RenderPassEncoderPushDebugGroup(engine.CurrentRenderPassEncoder, "Terrain Scene");
                // terrain.Render();
                foreach (Crate crate in crates)
                {
                    // crate.Render();
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
