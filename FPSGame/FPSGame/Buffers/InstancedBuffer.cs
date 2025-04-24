using FPSGame.Utils;
using WGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace FPSGame.Buffers
{
    public unsafe class InstanceBuffer<T> : IDisposable where T : unmanaged
    {
        private readonly Engine engine;

        public InstanceBuffer(Engine engine)
        {
            this.engine = engine;
        }

        public WGPUBuffer* Buffer { get; private set; }
        public uint Size { get; private set; }
        public uint InstanceCount { get; private set; }

        public void Initialize(T[] data)
        {
            Size = (uint)data.Length * (uint)sizeof(T);
            Buffer = WebGPUUtil.Buffer.CreateInstanceBuffer(engine, data);
            InstanceCount = (uint)data.Length;
        }

        public void Update(T data, int instanceIndex)
        {
            uint offset = (uint)instanceIndex * (uint)sizeof(T);
            engine.WGPU.QueueWriteBuffer(engine.Queue, Buffer, offset, &data, (uint)sizeof(T));
        }

        public void Dispose()
        {
            engine.WGPU.BufferDestroy(Buffer);
        }
    }
}
