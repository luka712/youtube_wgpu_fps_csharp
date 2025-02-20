using FPSGame.Utils;
using Silk.NET.Maths;

namespace FPSGame.Texture;

using Silk.NET.WebGPU;
using SkiaSharp;
using System;
using WGPUBuffer = Silk.NET.WebGPU.Buffer;

public unsafe class Texture2D : IDisposable
{
    private readonly Engine engine;
    private SKImage? image;
    private byte[] data;
    private Vector2D<uint> size;
    private bool canBeReadFrom;

    public Texture2D(Engine engine, SKImage image, string label = "Texture2D", bool canBeReadFrom = false)
    {
        this.engine = engine;
        this.image = image;
        this.canBeReadFrom = canBeReadFrom;
        Label = label;
    }

    public Texture2D(Engine engine, Vector2D<uint> size, byte[] data, string label = "Texture2D")
    {
        this.engine = engine;
        this.data = data;
        this.size = size;
        Label = label;
    }

    public Texture* Texture { get; private set; }

    public TextureView* TextureView { get; private set; }

    public Sampler* Sampler { get; private set; }

    public string Label { get; private set; }

    public uint Width => size.X;

    public uint Height => size.Y;

    public void Initialize()
    {
        if (image != null)
        {
            TextureUsage usage = TextureUsage.TextureBinding | TextureUsage.CopyDst;
            if(canBeReadFrom)
            {
                usage |= TextureUsage.CopySrc;
            }
            Texture = WebGPUUtil.Texture.Create(engine, image, Label, usage);
            size = new Vector2D<uint>((uint)image.Width, (uint)image.Height);
        }
        else
        {
            Texture = WebGPUUtil.Texture.Create(engine, data!, size.X, size.Y, Label);
        }

        TextureView = WebGPUUtil.TextureView.Create(engine, Texture, label: Label);
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

    public byte[] GetPixels()
    {
        uint width = size.X;
        uint height = size.Y;
        uint paddedBytesPerRow = (uint)(((width * 4) + 255) & ~255);

        // - CREATE TEMPORARY BUFFER TO READ FROM
        WGPUBuffer* stagingBuffer = WebGPUUtil.Buffer.CreateStagingBuffer(engine, paddedBytesPerRow * height);

        // - COPY TEXTURE TO STAGING BUFFER
        WebGPUUtil.Buffer.CopyTextureToBuffer(engine, Texture, stagingBuffer, paddedBytesPerRow, width, height);

        uint byteSize = paddedBytesPerRow * height;
        byte[] data = new byte[width * height * 4];
        WebGPUUtil.Buffer.Read(engine, stagingBuffer, byteSize, ptr =>
        {
            byte* dataPtr = (byte*)ptr;

            for (int y = 0; y < height; y++)
            {
                int row = (int)width * 4 * y;
                int paddedRow = (int)paddedBytesPerRow * y;

                for (int x = 0; x < width; x++)
                {
                    int destIndex = row + x * 4;
                    int srcIndex = paddedRow + x * 4;

                    data[destIndex + 0] = dataPtr[srcIndex + 0]; // R
                    data[destIndex + 1] = dataPtr[srcIndex + 1]; // G
                    data[destIndex + 2] = dataPtr[srcIndex + 2]; // B
                    data[destIndex + 3] = dataPtr[srcIndex + 3]; // A
                }
            }
        });

        engine.WGPU.BufferRelease(stagingBuffer);

        return data;
    }

    public void Dispose()
    {
        engine.WGPU.SamplerRelease(Sampler);
        engine.WGPU.TextureViewRelease(TextureView);
        engine.WGPU.TextureRelease(Texture);
    }
}