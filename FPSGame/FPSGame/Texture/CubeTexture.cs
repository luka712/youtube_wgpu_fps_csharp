using System.Numerics;
using System.Runtime.InteropServices;
using FPSGame.Utils;
using Silk.NET.Maths;

namespace FPSGame.Texture;

using Silk.NET.WebGPU;
using SkiaSharp;

public unsafe class CubeTexture : IDisposable
{
    private readonly Engine engine;

    private readonly SKImage imageRight;
    private readonly SKImage imageLeft;
    private readonly SKImage imageTop;
    private readonly SKImage imageBottom;
    private readonly SKImage imageFront;
    private readonly SKImage imageBack;


    public CubeTexture(Engine engine,
        SKImage imageRight,
        SKImage imageLeft,
        SKImage imageTop,
        SKImage imageBottom,
        SKImage imageFront,
        SKImage imageBack,
        string label = "CubeTexture")
    {
        this.engine = engine;
        this.imageRight = imageRight;
        this.imageLeft = imageLeft;
        this.imageTop = imageTop;
        this.imageBottom = imageBottom;
        this.imageFront = imageFront;
        this.imageBack = imageBack;
        Label = label;
    }


    public Texture* Texture { get; private set; }

    public TextureView* TextureView { get; private set; }

    public Sampler* Sampler { get; private set; }

    public string Label { get; private set; }

    public void Initialize()
    {

        Texture = WebGPUUtil.Texture.CreateCubeTexture(engine, imageLeft, imageRight, imageTop, imageBottom, imageFront, imageBack, Label);

        TextureView = WebGPUUtil.TextureView.CreateCubeTextureView(engine, Texture, label: Label);
        Sampler = WebGPUUtil.Sampler.Create(engine, Label);
    }

    public static Texture2D FromFile(
        Engine engine,
        SKImage image,
        string label = "Texture2D")
    {
        Texture2D texture = new(engine, image, label);
        texture.Initialize();
        return texture;
    }

    public static Texture2D CreateEmptyTexture(Engine engine, string label = "Empty Texture2D")
    {
        Texture2D texture = new(engine, new(1, 1), [255, 255, 255, 255], label);
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