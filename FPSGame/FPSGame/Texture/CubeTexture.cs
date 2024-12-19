using FPSGame.Utils;
using Silk.NET.WebGPU;
using SkiaSharp;

namespace FPSGame.Texture;

public unsafe class CubeTexture : IDisposable
{
    private readonly Engine engine;
    private readonly SKImage leftImage;
    private readonly SKImage rightImage;
    private readonly SKImage topImage;
    private readonly SKImage bottomImage;
    private readonly SKImage frontImage;
    private readonly SKImage backImage;
    
    public CubeTexture(
        Engine engine, 
        SKImage rightImage,
        SKImage leftImage,
        SKImage topImage,
        SKImage bottomImage,
        SKImage frontImage,
        SKImage backImage,
        string label = "CubeTexture")
    {
        this.engine = engine;
        this.leftImage = leftImage;
        this.rightImage = rightImage;
        this.topImage = topImage;
        this.bottomImage = bottomImage;
        this.frontImage= frontImage;
        this.backImage = backImage;
        Label = label;
    }
    
    
    public string Label { get; private set; }
    
    public Silk.NET.WebGPU.Texture* Texture { get; private set; }
    
    public TextureView* TextureView { get; private set; }
    
    public Sampler* Sampler { get; private set; }

    public void Initialize()
    {
        Texture = WebGPUUtil.Texture.CreateCubeTexture(engine,
            rightImage,
            leftImage,
            topImage,
            bottomImage,
            frontImage,
            backImage,
            Label);

        TextureView = WebGPUUtil.TextureView.CreateCubeTexture(engine, Texture, label: Label);
        Sampler = WebGPUUtil.Sampler.Create(engine, Label);
    }

    public void Dispose()
    {
        engine.WGPU.SamplerRelease(Sampler);
        engine.WGPU.TextureViewRelease(TextureView);
        engine.WGPU.TextureRelease(Texture);
    }
}