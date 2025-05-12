using FPSGame.Scene;
using FPSGame;
using WebGPU_FPS_Game.GameObjects;
using BulletSharp;
using WebGPU_FPS_Game.DebugObjects;
using FPSGame.Camera;
using FPSGame.GameObject;
using Silk.NET.Maths;

namespace WebGPU_FPS_Game.Scene
{
    public class TerrainScene(Engine engine, DiscreteDynamicsWorld world) : BaseScene
    {
        const bool DEBUG = false;

        Skybox skybox = new(engine);
        Terrain terrain = new(engine, world);
        List<Crate> crates = new();
        FPSCamera camera = null!;
        Player player = new(engine);
        BulletDebugDrawable bulletDebugDrawable = null!;
        Decal treeDecals = new(engine);
        Enemy enemy = new(engine);


        public override void Initialize()
        {
            camera = new FPSCamera(engine);
            camera.Position = new(0, 0, -3);
            camera.AspectRatio = engine.Window.Size.X / (float)engine.Window.Size.Y;

            player.Initialize(camera, world);

            skybox.Initialize(camera);
            terrain.Initialize(camera);
            treeDecals.Initialize(camera, terrain);
            enemy.Initialize(camera, terrain);
            enemy.Animation = AnimationState.Walk;

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
            enemy.Update();
            foreach (Crate crate in crates)
            {
                crate.Update();
            }

            // Move enemy towards player.
            var direction = player.Position - enemy.Position;
            if (direction.LengthSquared > 0)
            {
                enemy.Position = enemy.Position + Vector2D.Normalize(direction) * 0.05f;
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
                enemy.Render();
                engine.WGPU.RenderPassEncoderPopDebugGroup(engine.CurrentRenderPassEncoder);
            }
        }

        public override void Dispose()
        {

        }
    }
}
