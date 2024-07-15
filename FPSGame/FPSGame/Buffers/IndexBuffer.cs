using FPSGame.Utils;

namespace FPSGame.Buffers;

using WGPUBuffer = Silk.NET.WebGPU.Buffer;

public unsafe class IndexBuffer : IDisposable
{
    private readonly Engine engine;

    public IndexBuffer(Engine engine)
    {
        this.engine = engine;
    }

    public WGPUBuffer* Buffer { get; private set; }
    public uint Size { get; private set; }
    public uint VertexCount { get; private set; }

    public void Initialize(ushort[] data)
    {
        Size = (uint) data.Length * sizeof(float);
        Buffer = WebGPUUtil.BufferUtil.CreateIndexBuffer(engine, data);
        VertexCount = (uint) data.Length;
    }

    public void Dispose()
    {
        engine.WGPU.BufferDestroy(Buffer);
    }
}