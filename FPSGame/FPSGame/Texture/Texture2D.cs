using System.Runtime.InteropServices;
using FPSGame.Utils;

namespace FPSGame.Texture;

using Silk.NET.WebGPU;
using SkiaSharp;

public unsafe class Texture2D : IDisposable
{
    private readonly Engine engine;
    private SKImage? image;
    
    public Texture2D(Engine engine, SKImage image, string label = "Texture2D")
    {
        this.engine = engine;
        this.image = image;
        Label = label;
    }
    
    public Texture* Texture { get; private set; }
    
    public TextureView* TextureView { get; private set; }
    
    public Sampler* Sampler { get; private set; }
    
    public string Label { get; private set; }

    public void Initialize()
    {
        if (image != null)
        {
            Texture = WebGPUUtil.Texture.Create(engine, image, Label);
        }
        
        TextureView = WebGPUUtil.TextureView.Create(engine, Texture, Label);
        Sampler = WebGPUUtil.Sampler.Create(engine, Label);
    }

    public static Texture2D FromFile(
        Engine engine,
        SKImage image, 
        string label = "Texture2D")
    {
        Texture2D texture = new (engine, image, label);
        texture.Initialize();
        return texture;
    }


    public void Dispose()
    {
        engine.WGPU.SamplerRelease(Sampler);
        engine.WGPU.TextureViewRelease(TextureView);
        engine.WGPU.TextureRelease(Texture);
    }
}