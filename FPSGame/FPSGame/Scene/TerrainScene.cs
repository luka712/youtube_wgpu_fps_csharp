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
        FPSCamera camera = null!;
        SKImage image = SKImage.FromEncodedData("Assets/RTS_Crate.png");
        Skybox skybox = new Skybox();
        List<Crate> crates = new List<Crate>();
        Terrain terrain = new();
        Texture2D? texture = null;
        Random rand = new();

        public override void Initialize()
        {
            if (image is null)
            {
                throw new FileNotFoundException("Unable to load image.");
            }

            Crate.StaticInitialize(engine);
            Terrain.StaticInitialize(engine);

            camera = new FPSCamera(engine);
            camera.Position = new(5, 5, 0);
            camera.AspectRatio = engine.Window.Size.X / (float)engine.Window.Size.Y;

            texture = new Texture2D(engine, image, "Texture2D");
            texture.Initialize();

            skybox.Initialize(engine, camera);
            terrain.Initialize(engine, world, camera);

            for (int i = 0; i < 40; i++)
            {
                Crate crate = new Crate();

                float x = rand.NextSingle() * 5 - 2.5f;
                float y = rand.NextSingle() * 5 + 5f;
                float z = rand.NextSingle() * 5 - 2.5f;


                crate.Initialize(engine, world, camera, texture, Matrix4X4.CreateTranslation(x, y, z));
                crates.Add(crate);
            }
        }

        public override void Update()
        {
            camera.Update();
            terrain.Update();
            crates.ForEach(crate => crate.Update());
        }

        public override void Render()
        {
            terrain.Render();
            crates.ForEach(crate => crate.Render());
            skybox.Render();
        }

        public override void Dispose()
        {
            texture?.Dispose();
        }


    }
}
