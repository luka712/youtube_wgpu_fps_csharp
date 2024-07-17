using FPSGame.Utils;
using WGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace FPSGame.Buffers
{
    public unsafe class IndexBuffer : IDisposable
    {
        private readonly Engine engine;

        public IndexBuffer(Engine engine)
        {
            this.engine = engine;
        }

        public WGPUBuffer* Buffer { get; private set; }

        public uint Size { get; private set; }
        
        public uint IndicesCount { get; private set; }

        public void Initialize(ushort[] data)
        {
            Size = (uint) data.Length * sizeof(ushort);
            Buffer = WebGPUUtil.Buffer.CreateIndexBuffer(engine, data);
            IndicesCount = (uint) data.Length;
        }

        public void Dispose()
        {
            engine.WGPU.BufferDestroy(Buffer);
        }
    }
}