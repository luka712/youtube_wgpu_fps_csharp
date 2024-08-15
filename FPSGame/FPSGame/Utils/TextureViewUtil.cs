using System.Runtime.InteropServices;

namespace FPSGame.Utils;

using Silk.NET.WebGPU;

public unsafe class TextureViewUtil
{
    public TextureView* Create(Engine engine, Texture* texture, string label = "TextureView")
    {
        TextureViewDescriptor desc = new();
        desc.Format = engine.PreferredTextureFormat;
        desc.Aspect = TextureAspect.All;
        desc.Dimension = TextureViewDimension.Dimension2D;
        desc.Label = (byte*)Marshal.StringToHGlobalAnsi(label);
        desc.MipLevelCount = 1;
        desc.BaseMipLevel = 0;
        desc.ArrayLayerCount = 1;
        desc.BaseArrayLayer = 0;

        Console.WriteLine("Creating texture view");
        TextureView* textureView = engine.WGPU.TextureCreateView(texture, desc);
        Console.WriteLine("Texture view created");
        return textureView;
    }
}