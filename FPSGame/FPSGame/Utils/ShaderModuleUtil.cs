using Silk.NET.WebGPU;
using System.Runtime.InteropServices;

namespace FPSGame.Utils
{
    public unsafe class ShaderModuleUtil
    {
        public ShaderModule* Create(Engine engine, string filePath)
        {
            if(!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            string shaderCode = File.ReadAllText(filePath);

            ShaderModuleWGSLDescriptor wgslDescriptor = new ShaderModuleWGSLDescriptor();
            wgslDescriptor.Code = (byte*)Marshal.StringToHGlobalAnsi(shaderCode);
            wgslDescriptor.Chain.SType = SType.ShaderModuleWgslDescriptor;

            ShaderModuleDescriptor descriptor = new ShaderModuleDescriptor();
            descriptor.NextInChain = (ChainedStruct*)&wgslDescriptor;

            ShaderModule* shaderModule = engine.WGPU.DeviceCreateShaderModule(engine.Device, descriptor);

            return shaderModule;
        }
    }
}
