using FPSGame.Utils;

using WGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace FPSGame.Buffers
{
    /// <summary>
    /// Global buffer.
    /// </summary>
    public unsafe class UniformBuffer<T> where T: unmanaged
    {
        private readonly Engine engine;

        public UniformBuffer(Engine engine)
        {
            this.engine = engine;
        }

        public WGPUBuffer* Buffer { get; private set; }
        public uint Size { get; private set; }

        public void Initialize(T data)
        {
            Size = (uint) sizeof(T);
            Buffer = WebGPUUtil.Buffer.CreateUniformBuffer(engine, data);
        }

        public void Write(T data)
        {
            WebGPUUtil.Buffer.WriteUniformBuffer(engine, Buffer, data);
        }

        public void Dispose()
        {
            engine.WGPU.BufferDestroy(Buffer);
        }
    }
}
