﻿using FPSGame.Utils;


using WGPUBuffer = Silk.NET.WebGPU.Buffer;

namespace FPSGame.Buffers
{
    public unsafe class VertexBuffer : IDisposable
    {
        private readonly Engine engine;

        public VertexBuffer(Engine engine)
        {
            this.engine = engine;
        }

        public WGPUBuffer* Buffer { get; private set; }

        public uint Size { get; private set; }

        public void Initialize(float[] data)
        {
            Size = (uint) data.Length * sizeof(float);
            Buffer = WebGPUUtil.BufferUtil.Create(engine, data);
        }

        public void Dispose()
        {
            engine.WGPU.BufferDestroy(Buffer);
        }

    }
}