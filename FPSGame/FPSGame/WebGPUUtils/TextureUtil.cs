using System.Runtime.InteropServices;
using FPSGame;
using Silk.NET.WebGPU;
using SkiaSharp;
using WGPUTexture = Silk.NET.WebGPU.Texture;

namespace FPS_Game.Utils;

public unsafe class TextureUtil
{
    public WGPUTexture* Create(Engine engine, SKImage image, string label = "Texture2D")
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

        WGPUTexture* texture = Create(engine, pixels, (uint)image.Width, (uint)image.Height, label);

        return texture;
    }

    public WGPUTexture* CreateCubeTexture(Engine engine,
        SKImage imageLeft,
        SKImage imageRight,
        SKImage imageTop,
        SKImage imageBottom,
        SKImage imageFront,
        SKImage imageBack,
        string label = "Texture2D")
    {
        TextureDescriptor descriptor = new();
        descriptor.Size = new Extent3D((uint)imageLeft.Width, (uint)imageLeft.Height, 6);
        descriptor.Dimension = TextureDimension.Dimension2D;
        descriptor.Format = engine.PreferredTextureFormat;
        descriptor.MipLevelCount = 1;
        descriptor.SampleCount = 1;
        descriptor.Usage = TextureUsage.TextureBinding | TextureUsage.CopyDst;

        WGPUTexture* texture = engine.WGPU.DeviceCreateTexture(engine.Device, descriptor);

        SKImage[] images = [imageRight, imageLeft, imageTop, imageBottom, imageFront, imageBack];

        // Arguments telling which part of the texture we upload to
        ImageCopyTexture destination = new ImageCopyTexture();
        destination.Texture = texture;
        destination.MipLevel = 0;
        destination.Origin = new Origin3D(0, 0, 0);
        destination.Aspect = TextureAspect.All;

        TextureDataLayout source = new TextureDataLayout();
        source.Offset = 0;

        for (int i = 0; i < images.Length; i++)
        {
            // We need to change destination origin z.
            destination.Origin.Z = (uint)i;

            // We need to set source rows per image and bytes per row.
            source.RowsPerImage = (uint)images[i].Height;
            source.BytesPerRow = 4 * (uint)images[i].Width;

            Extent3D size = new((uint)images[i].Width, (uint)images[i].Height, 1);

            // First we need image pixels.
            SKImage image = images[i];
            SKImageInfo imageInfo = new SKImageInfo(image.Width, image.Height);
            int[] pixels = new int[image.Width * image.Height];

            fixed (int* pixelsPtr = pixels)
            {
                image.ReadPixels(imageInfo, (IntPtr)pixelsPtr);
                engine.WGPU.QueueWriteTexture(engine.Queue, &destination, pixelsPtr, (uint)pixels.Length * 4, in source, in size);
            }
        }

        return texture;
    }

    public WGPUTexture* Create<T>(Engine engine, T[] data, uint width, uint height, string label = "Texture2D")
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
        WGPUTexture* texture = engine.WGPU.DeviceCreateTexture(engine.Device, descriptor);
        Console.WriteLine("Texture created");

        Write(engine, texture, data, width, height);

        return texture;
    }

    public WGPUTexture* CreateDepthTexture(Engine engine, uint width, uint height)
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

    public void Write<T>(Engine engine, WGPUTexture* texture, T[] pixels, uint width, uint height, uint originZ = 0)
        where T : unmanaged
    {
        Console.WriteLine("Reading pixels from image");

        // Write to texture
        ImageCopyTexture desination = new();
        desination.Texture = texture;
        desination.MipLevel = 0;
        desination.Origin = new Origin3D(0, 0, originZ);
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