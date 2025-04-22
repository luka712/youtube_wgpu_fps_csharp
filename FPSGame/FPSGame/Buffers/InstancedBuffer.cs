using FPSGame.Utils;
using WGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace FPSGame.Buffers
{
    public unsafe class InstanceBuffer<T> : IDisposable where T : unmanaged
    {
        private readonly Engine engine;

        public InstanceBuffer(Engine engine, string label = "")
        {
            this.engine = engine;
            Label = label;
        }

        public string Label { get; }

        public uint InstanceCount { get; private set; }

        public WGPUBuffer* Buffer { get; private set; }
        public uint Size { get; private set; }

        public void Initialize(T[] data)
        {
            InstanceCount = (uint)data.Length;
            Size = (uint)data.Length * (uint)sizeof(T);
            Buffer = WebGPUUtil.Buffer.CreateInstanceBuffer(engine, data, Label);
        }

        /// <summary>
        /// Updates the buffer.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Update(T data, uint instance)
        {
            uint size = (uint)sizeof(T);
            engine.WGPU.QueueWriteBuffer(engine.Queue, Buffer, (ulong)(sizeof(T) * instance), &data, size);

        }

        public void Dispose()
        {
            engine.WGPU.BufferDestroy(Buffer);
        }
    }
}
