using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Pipelines;
using FPSGame.Texture;
using Silk.NET.Maths;
using SkiaSharp;

namespace FPSGame.Scene;

public class SimpleQuadScene(Engine engine) : BaseScene
{
    VertexBuffer vertexBuffer;
    IndexBuffer indexBuffer;
    PerspectiveCamera camera = null!;
    UnlitRenderPipeline unlitRenderPipeline = null!;

    SKImage image = SKImage.FromEncodedData("Assets/test.png");
    Texture2D? texture = null;

    float rotation = 0;
    

    public override void Initialize()
    {
        vertexBuffer = new VertexBuffer(engine);
        indexBuffer = new IndexBuffer(engine);
        
        if (image is null)
        {
            throw new FileNotFoundException("Unable to load image.");
        }
    
        camera = new PerspectiveCamera(engine);
        camera.Position = new(0, 0, 3);
        camera.AspectRatio = engine.Window.Size.X / (float) engine.Window.Size.Y;

        unlitRenderPipeline = new UnlitRenderPipeline(engine, camera, "Unlit Render Pipeline");

        texture = new Texture2D(engine, image, "Texture2D");
        texture.Initialize();

        unlitRenderPipeline.Initialize();
        unlitRenderPipeline.Texture = texture;


        vertexBuffer.Initialize(new float[]
        {
            -0.5f, -0.5f, 0f,    1, 1, 1, 1,  0,1, // v0
            0.5f, -0.5f, 0f,    1, 1, 1, 1,   1,1,// v1
            -0.5f,  0.5f, 0f,    1, 1, 1, 1,  0,0,// v2
            0.5f,  0.5f, 0f,    1, 1, 1, 1,    1,0// v3
        }, 6);

        indexBuffer.Initialize(new ushort[]
        {
            0,1,2, // t0
            1,3,2  // t1
        });
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