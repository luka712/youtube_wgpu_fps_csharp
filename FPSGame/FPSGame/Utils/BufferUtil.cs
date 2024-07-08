using Silk.NET.WebGPU;

using WGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace FPSGame
{
    internal unsafe class BufferUtil
    {

        internal WGPUBuffer* Create(Engine engine, float[] data)
        {
            BufferDescriptor bufferDescriptor = new BufferDescriptor();
            bufferDescriptor.MappedAtCreation = false;
            uint size = (uint)data.Length * sizeof(float);
            bufferDescriptor.Size = size;
            bufferDescriptor.Usage = BufferUsage.CopyDst | BufferUsage.Vertex;

            WGPUBuffer* buffer = engine.WGPU.DeviceCreateBuffer(engine.Device, bufferDescriptor);

            fixed(float* dataPtr = data)
            {
                engine.WGPU.QueueWriteBuffer(engine.Queue, buffer, 0, dataPtr, size);
            }

            return buffer;
        }
    }
}
