using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Pipelines;
using FPSGame.Texture;
using Silk.NET.Maths;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPSGame.Scene
{
    internal class CubeTestScene(Engine engine) : BaseScene
    {
        PerspectiveCamera camera = null!;
        UnlitRenderPipeline unlitRenderPipeline = null!;
        VertexBuffer vertexBuffer = new VertexBuffer(engine);
        IndexBuffer indexBuffer = new IndexBuffer(engine);
        SKImage image = SKImage.FromEncodedData("Assets/RTS_Crate.png");
        Texture2D? texture = null;
        float rotation = 0;

        public override void Initialize()
        {
            if (image is null)
            {
                throw new FileNotFoundException("Unable to load image.");
            }

            camera = new PerspectiveCamera(engine);
            camera.Position = new(0, 0, 3);
            camera.AspectRatio = engine.Window.Size.X / (float)engine.Window.Size.Y;

            unlitRenderPipeline = new UnlitRenderPipeline(engine, camera, "Unlit Render Pipeline");

            texture = new Texture2D(engine, image, "Texture2D");
            texture.Initialize();

            unlitRenderPipeline.Initialize();
            unlitRenderPipeline.Texture = texture;

            Geometry geometry = GeometryBuilder.CreateCubeGeometry();

            vertexBuffer.Initialize(geometry.InterleavedVertices, geometry.VertexCount);

            indexBuffer.Initialize(geometry.Indices);
        }

        public override void Render()
        {
            camera.Update();

            // Temp
            rotation += 0.01f;
            unlitRenderPipeline.Transform = Matrix4X4.CreateRotationY(rotation);

            unsafe
            {
                engine.WGPU.RenderPassEncoderPushDebugGroup(engine.CurrentRenderPassEncoder, "Unlit Render Pipeline");
                unlitRenderPipeline.Render(vertexBuffer, indexBuffer);
                engine.WGPU.RenderPassEncoderPopDebugGroup(engine.CurrentRenderPassEncoder);
            }
        }

        public override void Dispose()
        {
            unlitRenderPipeline?.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            texture?.Dispose();
        }
    }
}
