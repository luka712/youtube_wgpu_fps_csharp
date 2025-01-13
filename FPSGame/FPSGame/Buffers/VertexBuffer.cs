using FPSGame.Utils;

using WGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace FPSGame.Buffers
{
    public unsafe class VertexBuffer : IDisposable
    {
        private readonly Engine engine;

        public VertexBuffer(Engine engine, string label = "")
        {
            this.engine = engine;
            Label = label;
        }

        public string Label { get; }

        public WGPUBuffer* Buffer { get; private set; }
        public uint Size { get; private set; }
        public uint VertexCount { get; private set; }

        public void Initialize(float[] data, uint vertexCount)
        {
            Size = (uint) data.Length * sizeof(float);
            Buffer = WebGPUUtil.Buffer.CreateVertexBuffer(engine, data, Label);
            VertexCount = vertexCount;
        }

        /// <summary>
        /// Updates the buffer.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="length">The length. If -1 the length of array is used.</param>
        public void Update(float[] data, int length = -1)
        {
            uint realLength = (uint)(length == -1 ? data.Length : length);

            fixed (float* dataPtr = data)
            {
                engine.WGPU.QueueWriteBuffer(engine.Queue, Buffer, 0, dataPtr, realLength);
            }
        }

        public void Dispose()
        {
            engine.WGPU.BufferDestroy(Buffer);
        }
    }
}
