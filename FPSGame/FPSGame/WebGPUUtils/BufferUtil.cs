using FPSGame.Extensions;
using Silk.NET.SDL;
using Silk.NET.WebGPU;
using System;
using System.Runtime.InteropServices;
using WGPUBuffer = Silk.NET.WebGPU.Buffer;
using WGPUTexture = Silk.NET.WebGPU.Texture;

namespace FPSGame
{
    public unsafe class BufferUtil
    {
        public WGPUBuffer* CreateVertexBuffer(Engine engine, float[] data, string label = "")
        {
            BufferDescriptor descriptor = new BufferDescriptor();
            descriptor.Label = label.ToBytePtr();
            descriptor.MappedAtCreation = false;
            uint size = (uint)data.Length * sizeof(float);
            descriptor.Size = size;
            descriptor.Usage = BufferUsage.Vertex | BufferUsage.CopyDst;

            WGPUBuffer* buffer = engine.WGPU.DeviceCreateBuffer(engine.Device, descriptor);

            fixed (float* dataPtr = data)
            {
                engine.WGPU.QueueWriteBuffer(engine.Queue, buffer, 0, dataPtr, size);
            }

            return buffer;
        }

        public WGPUBuffer* CreateIndexBuffer(Engine engine, ushort[] data)
        {
            BufferDescriptor descriptor = new BufferDescriptor();
            descriptor.MappedAtCreation = false;
            uint size = (uint)data.Length * sizeof(ushort);
            descriptor.Size = size;
            descriptor.Usage = BufferUsage.Index | BufferUsage.CopyDst;

            WGPUBuffer* buffer = engine.WGPU.DeviceCreateBuffer(engine.Device, descriptor);

            fixed (ushort* dataPtr = data)
            {
                engine.WGPU.QueueWriteBuffer(engine.Queue, buffer, 0, dataPtr, size);
            }

            return buffer;
        }

        public WGPUBuffer* CreateUniformBuffer<T>(Engine engine, T data, string label = "") where T : unmanaged
        {
            BufferDescriptor descriptor = new BufferDescriptor();
            descriptor.Label = label.ToBytePtr();
            descriptor.MappedAtCreation = false;
            uint size = (uint)sizeof(T);
            descriptor.Size = size;
            descriptor.Usage = BufferUsage.Uniform | BufferUsage.CopyDst;

            WGPUBuffer* buffer = engine.WGPU.DeviceCreateBuffer(engine.Device, descriptor);
            engine.WGPU.QueueWriteBuffer(engine.Queue, buffer, 0, data, size);

            return buffer;
        }

        public WGPUBuffer* CreateStagingBuffer(Engine engine, uint size)
        {
            BufferDescriptor descriptor = new BufferDescriptor();
            descriptor.MappedAtCreation = false;
            descriptor.Size = size;
            descriptor.Usage = BufferUsage.MapRead | BufferUsage.CopyDst;

            return engine.WGPU.DeviceCreateBuffer(engine.Device, descriptor);
        }

        public void WriteUniformBuffer<T>(Engine engine, WGPUBuffer* buffer, T data) where T : unmanaged
        {
            engine.WGPU.QueueWriteBuffer(engine.Queue, buffer, 0, data, (uint)sizeof(T));
        }

        public void CopyTextureToBuffer(Engine engine, WGPUTexture* sourceTexture, WGPUBuffer* destinationBuffer, uint paddedBytesPerRow, uint width, uint height)
        {
            CommandEncoder* blitEncoder = engine.WGPU.DeviceCreateCommandEncoder(engine.Device, null);

            ImageCopyTexture source = new();
            source.Texture = sourceTexture;
            source.MipLevel = 0;
            source.Origin = new Origin3D(0, 0, 0);

            ImageCopyBuffer destination = new();
            destination.Buffer = destinationBuffer;
            destination.Layout = new()
            {
                Offset = 0,
                BytesPerRow = paddedBytesPerRow, 
                RowsPerImage = height, 
            };

            Extent3D copySize = new(width, height, 1);

            engine.WGPU.CommandEncoderCopyTextureToBuffer(blitEncoder, source, destination, copySize);

            // - EXECUTE COMMANDS
            CommandBuffer* commandBuffer = engine.WGPU.CommandEncoderFinish(blitEncoder, null);
            engine.WGPU.QueueSubmit(engine.Queue, 1, &commandBuffer);

            engine.WGPU.CommandBufferRelease(commandBuffer);
            engine.WGPU.CommandEncoderRelease(blitEncoder);
        }

        public void Read(Engine engine, WGPUBuffer* buffer, uint byteSize, Action<IntPtr> action)
        {
            bool isRead = false;
            PfnBufferMapCallback callback = PfnBufferMapCallback.From((status, msgPtr) =>
            {
                if (status != BufferMapAsyncStatus.Success)
                {
                    string msg = Marshal.PtrToStringAnsi((IntPtr)msgPtr)!;
                    Console.WriteLine(msg);
                    return;
                }

                isRead = true;
                IntPtr dataPtr = (IntPtr)engine.WGPU.BufferGetMappedRange(buffer, 0, byteSize);
                action(dataPtr);
            });
            engine.WGPU.BufferMapAsync(buffer, MapMode.Read, 0, byteSize, callback, null);
            while (!isRead)
            {
                // Keep submitting queue until read.
                engine.WGPU.QueueSubmit(engine.Queue, 0, null);
            }
            engine.WGPU.BufferUnmap(buffer);
        }
    }
}