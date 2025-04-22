using FPSGame.Scene;
using FPSGame;
using WebGPU_FPS_Game.GameObjects;
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
        Player player = new(engine);
        BulletDebugDrawable bulletDebugDrawable = null!;
        Decal treeDecals = new(engine);


        public override void Initialize()
        {
            camera = new FPSCamera(engine);
            camera.Position = new(0, 0, -3);
            camera.AspectRatio = engine.Window.Size.X / (float)engine.Window.Size.Y;

            player.Initialize(camera, world);

            skybox.Initialize(camera);
            terrain.Initialize(camera);
            treeDecals.Initialize(camera, terrain);

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
            player.Update();
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
                terrain.Render();
                foreach (Crate crate in crates)
                {
                    crate.Render();
                }
                skybox.Render();
                treeDecals.Render();
                engine.WGPU.RenderPassEncoderPopDebugGroup(engine.CurrentRenderPassEncoder);
            }
        }

        public override void Dispose()
        {

        }
    }
}
