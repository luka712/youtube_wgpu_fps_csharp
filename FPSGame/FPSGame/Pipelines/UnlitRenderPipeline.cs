using FPSGame.Buffers;
using FPSGame.Utils;
using Silk.NET.WebGPU;

namespace FPSGame.Pipelines;


public unsafe class UnlitRenderPipeline : IDisposable
{
    private readonly Engine engine;
    private RenderPipeline* renderPipeline;

    public UnlitRenderPipeline(Engine engine)
    {
        this.engine = engine;
    }

    public void Initialize()
    {
        ShaderModule* shaderModule = WebGPUUtil.ShaderModule.Create(engine, "Shaders/unlit.wgsl");
        renderPipeline = WebGPUUtil.RenderPipeline.Create(engine, shaderModule);
        engine.WGPU.ShaderModuleRelease(shaderModule);
    }

    public void Render(VertexBuffer vertexBuffer)
    {
        engine.WGPU.RenderPassEncoderSetPipeline(engine.CurrentRenderPassEncoder, renderPipeline);

        // Set buffers.
        engine.WGPU.RenderPassEncoderSetVertexBuffer(engine.CurrentRenderPassEncoder, 0, vertexBuffer.Buffer, 0, vertexBuffer.Size);

        engine.WGPU.RenderPassEncoderDraw(
            engine.CurrentRenderPassEncoder, 
            vertexBuffer.VertexCount, 
            1, 0, 0);
    }
    
    public void Render(VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
    {
        engine.WGPU.RenderPassEncoderSetPipeline(engine.CurrentRenderPassEncoder, renderPipeline);

        // Set buffers.
        engine.WGPU.RenderPassEncoderSetVertexBuffer(
            engine.CurrentRenderPassEncoder, 
            0, 
            vertexBuffer.Buffer, 
            0, 
            vertexBuffer.Size);
        
        engine.WGPU.RenderPassEncoderSetIndexBuffer(
            engine.CurrentRenderPassEncoder, 
            indexBuffer.Buffer, 
            IndexFormat.Uint16, 
            0, 
            indexBuffer.Size);

        engine.WGPU.RenderPassEncoderDrawIndexed(
            engine.CurrentRenderPassEncoder, 
            indexBuffer.VertexCount, 
            1, 0, 0, 0);
    }

    public void Dispose()
    {
        engine.WGPU.RenderPipelineRelease(renderPipeline);
    }
}