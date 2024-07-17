using Silk.NET.WebGPU;

using WGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace FPSGame
{
    public unsafe class BufferUtil
    {
        public WGPUBuffer* CreateVertexBuffer(Engine engine, float[] data)
        {
            BufferDescriptor descriptor = new BufferDescriptor();
            descriptor.MappedAtCreation = false;
            uint size = (uint) data.Length * sizeof(float);
            descriptor.Size = size;
            descriptor.Usage = BufferUsage.Vertex | BufferUsage.CopyDst;

            WGPUBuffer* buffer = engine.WGPU.DeviceCreateBuffer(engine.Device, descriptor);

            fixed(float* dataPtr = data)
            {
                engine.WGPU.QueueWriteBuffer(engine.Queue, buffer, 0, dataPtr, size);
            }

            return buffer;
        }
        
        public WGPUBuffer* CreateIndexBuffer(Engine engine, ushort[] data)
        {
            BufferDescriptor descriptor = new BufferDescriptor();
            descriptor.MappedAtCreation = false;
            uint size = (uint) data.Length * sizeof(ushort);
            descriptor.Size = size;
            descriptor.Usage = BufferUsage.Index | BufferUsage.CopyDst;

            WGPUBuffer* buffer = engine.WGPU.DeviceCreateBuffer(engine.Device, descriptor);

            fixed(ushort* dataPtr = data)
            {
                engine.WGPU.QueueWriteBuffer(engine.Queue, buffer, 0, dataPtr, size);
            }

            return buffer;
        }
    }
}
