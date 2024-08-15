using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

namespace FPSGame.Utils;

public unsafe class SamplerUtil
{
    public Sampler* Create(Engine engine, string label = "Sampler")
    {
        SamplerDescriptor desc = new();
        desc.MaxAnisotropy = 1;

        Console.WriteLine("Creating sampler");
        Sampler* sampler = engine.WGPU.DeviceCreateSampler(engine.Device, desc);
        Console.WriteLine("Sampler created");
        
        return sampler;
    }
}