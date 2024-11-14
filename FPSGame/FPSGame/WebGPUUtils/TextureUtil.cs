using System.Runtime.InteropServices;
using FPSGame;
using Silk.NET.WebGPU;
using SkiaSharp;

namespace FPS_Game.Utils;

public unsafe class TextureUtil
{
    public Texture* Create(Engine engine, SKImage image, string label = "Texture2D")
    {
        Console.WriteLine("Reading pixels from image");

        // First we need image pixels.
        SKImageInfo imageInfo = new SKImageInfo(image.Width, image.Height);
        int[] pixels = new int[image.Width * image.Height];

        fixed (int* pixelsPtr = pixels)
        {
            image.ReadPixels(imageInfo, (IntPtr)pixelsPtr);
        }

        Console.WriteLine("Pixels read");

        Texture* texture = Create(engine, pixels, (uint)image.Width, (uint)image.Height, label);


        return texture;
    }

    public Texture* Create<T>(Engine engine, T[] data, uint width, uint height, string label = "Texture2D")
         where T : unmanaged
    {
        TextureDescriptor descriptor = new();
        descriptor.Size = new Extent3D(width, height, 1);
        descriptor.Dimension = TextureDimension.Dimension2D;
        descriptor.Format = engine.PreferredTextureFormat;
        descriptor.MipLevelCount = 1;
        descriptor.SampleCount = 1;
        // TextureBinding - can be used in bind groups/shaders. 
        // CopyDst - can be used as the destination of a copy operation, or written into.
        descriptor.Usage = TextureUsage.TextureBinding | TextureUsage.CopyDst;
        descriptor.Label = (byte*)Marshal.StringToHGlobalAnsi(label);

        Console.WriteLine($"Creating texture with width: {width}, height: {height}");
        Texture* texture = engine.WGPU.DeviceCreateTexture(engine.Device, descriptor);
        Console.WriteLine("Texture created");

        Write(engine, texture, data, width, height);

        return texture;
    }

    public Texture* CreateDepthTexture(Engine engine, uint width, uint height)
    {
        TextureDescriptor descriptor = new()
        {
            SampleCount = 1,
            MipLevelCount = 1,
            Dimension = TextureDimension.Dimension2D,
            Size = new Extent3D(width, height, 1),
            Format = TextureFormat.Depth24PlusStencil8,
            Usage = TextureUsage.RenderAttachment,
        };

        return engine.WGPU.DeviceCreateTexture(engine.Device, descriptor);
    }

    public void Write<T>(Engine engine, Texture* texture, T[] pixels, uint width, uint height)
        where T : unmanaged
    {
        Console.WriteLine("Reading pixels from image");

        // Write to texture
        ImageCopyTexture desination = new();
        desination.Texture = texture;
        desination.MipLevel = 0;
        desination.Origin = new Origin3D(0, 0, 0);
        desination.Aspect = TextureAspect.All;

        // Image layout
        TextureDataLayout source = new();
        source.Offset = 0;
        source.BytesPerRow = 4 * width; // rgba * image.width
        source.RowsPerImage = height;

        Extent3D extent = new(width, height, 1);
        uint byteSize = width * height * 4; // rgba

        fixed (T* pixelsPtr = pixels)
        {
            engine.WGPU.QueueWriteTexture(
                engine.Queue,
                desination,
                pixelsPtr,
                byteSize,
                source,
                extent
            );
        }
    }
}