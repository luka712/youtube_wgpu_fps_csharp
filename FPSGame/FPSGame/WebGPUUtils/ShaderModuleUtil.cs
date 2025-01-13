using FPSGame.Extensions;
using Silk.NET.WebGPU;
using System.Runtime.InteropServices;

namespace FPSGame.Utils
{
    public unsafe class ShaderModuleUtil
    {
        public ShaderModule* Create(Engine engine, string filePath, string label = "")
        {
            if(!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            engine.WGPU.DevicePushErrorScope(engine.Device, ErrorFilter.Validation);

            string shaderCode = File.ReadAllText(filePath);

            ShaderModuleWGSLDescriptor wgslDescriptor = new ShaderModuleWGSLDescriptor();
            wgslDescriptor.Code = (byte*)Marshal.StringToHGlobalAnsi(shaderCode);
            wgslDescriptor.Chain.SType = SType.ShaderModuleWgslDescriptor;

            ShaderModuleDescriptor descriptor = new ShaderModuleDescriptor();
            descriptor.Label = label.ToBytePtr();
            descriptor.NextInChain = (ChainedStruct*)&wgslDescriptor;

            ShaderModule* shaderModule = engine.WGPU.DeviceCreateShaderModule(engine.Device, descriptor);

            engine.WGPU.DevicePopErrorScope(engine.Device, PfnErrorCallback.From((errorType, messagePtr, userData) =>
            {
                if (errorType != ErrorType.NoError && messagePtr != null)
                {
                    string msg = Marshal.PtrToStringAnsi((IntPtr)messagePtr)!;
                    msg = $"Validation error occurred. Error msg: {msg}";
                    Console.WriteLine(msg);
                    throw new InvalidOperationException(msg);
                }

            }), null);

            Marshal.FreeHGlobal((IntPtr) wgslDescriptor.Code);

            return shaderModule;
        }
    }
}
