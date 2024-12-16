using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Pipelines;
using FPSGame.Texture;
using Silk.NET.Maths;
using SkiaSharp;


namespace FPSGame.Scene
{
    internal class SkyboxTestScene(Engine engine) : BaseScene()
    {
        SkyboxRenderPipeline skyboxRenderPipeline = null!;
        FPSCamera camera = null!;
        UnlitRenderPipeline unlitRenderPipeline = null!;
        VertexBuffer vertexBuffer = new VertexBuffer(engine);
        IndexBuffer indexBuffer = new IndexBuffer(engine);
        SKImage image = SKImage.FromEncodedData("Assets/RTS_Crate.png");
        SKImage skyboxLeft = SKImage.FromEncodedData("Assets/xneg.png");
        SKImage skyboxRight = SKImage.FromEncodedData("Assets/xpos.png");
        SKImage skyboxTop = SKImage.FromEncodedData("Assets/ypos.png");
        SKImage skyboxBottom = SKImage.FromEncodedData("Assets/yneg.png");
        SKImage skyboxFront = SKImage.FromEncodedData("Assets/zpos.png");
        SKImage skyboxBack = SKImage.FromEncodedData("Assets/zneg.png");
        Texture2D? texture = null;
        float rotation = 0;

        public override void Initialize()
        {
            if (image is null)
            {
                throw new FileNotFoundException("Unable to load image.");
            }

            camera = new FPSCamera(engine);
            camera.Position = new(0, 0, -3);
            camera.AspectRatio = engine.Window.Size.X / (float)engine.Window.Size.Y;

            unlitRenderPipeline = new UnlitRenderPipeline(engine, camera, "Unlit Render Pipeline");

            texture = new Texture2D(engine, image, "Texture2D");
            texture.Initialize();

            unlitRenderPipeline.Initialize();
            unlitRenderPipeline.Texture = texture;

            Geometry cubeGeometry = GeometryBuilder.CreateCubeGeometry();

            // VertexCount is not relevant, since we draw with indices.
            vertexBuffer.Initialize(cubeGeometry.InterleavedVertices, cubeGeometry.VertexCount);
            indexBuffer.Initialize(cubeGeometry.Indices);

            // SKYBOX
            CubeTexture skyboxTexture = new CubeTexture(
                engine,
                skyboxRight,
                skyboxLeft,
                skyboxTop,
                skyboxBottom,
                skyboxFront,
                skyboxBack,
                "Skybox Texture"
            );
            skyboxTexture.Initialize();
            skyboxRenderPipeline = new SkyboxRenderPipeline(engine, camera, skyboxTexture, "Skybox Render Pipeline");
            skyboxRenderPipeline.Initialize();
        }

        public override void Update()
        {
            camera.Update();
        }

        public override void Render()
        {
            // Temp
            rotation += 0.01f;
            unlitRenderPipeline.Transform = Matrix4X4.CreateRotationY(rotation);

            unsafe
            {
                engine.WGPU.RenderPassEncoderPushDebugGroup(engine.CurrentRenderPassEncoder, "Unlit Render Pipeline");
                unlitRenderPipeline.Render(vertexBuffer, indexBuffer);
                skyboxRenderPipeline.Render();
                engine.WGPU.RenderPassEncoderPopDebugGroup(engine.CurrentRenderPassEncoder);
            }
        }

        public override void Dispose()
        {
            skyboxRenderPipeline?.Dispose();
            unlitRenderPipeline?.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            texture?.Dispose();
        }
    }
}
