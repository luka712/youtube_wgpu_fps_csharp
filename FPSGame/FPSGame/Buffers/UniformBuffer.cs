using FPSGame.Utils;

using WGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace FPSGame.Buffers
{
    public unsafe class UniformBuffer<T> : IDisposable where T : unmanaged 
    {
        private readonly Engine engine;

        public UniformBuffer(Engine engine, string label = "")
        {
            this.engine = engine;
            Label = label;
        }
        
        public string Label { get; private set; }

        public WGPUBuffer* Buffer { get; private set; }
        public uint Size { get; private set; }

        public void Initialize(T data)
        {
            Size = (uint) sizeof(T);
            Buffer = WebGPUUtil.Buffer.CreateUniformBuffer(engine, data, Label);
        }

        public void Update(T data)
        {
            WebGPUUtil.Buffer.WriteUniformBuffer(engine, Buffer, data);
        }

        public void Dispose()
        {
            engine.WGPU.BufferDestroy(Buffer);
        }
    }
}
