using FPSGame.Buffers;
using FPSGame.Utils;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System.Runtime.InteropServices;

namespace FPSGame.Pipelines;

public unsafe class UnlitRenderPipeline : IDisposable
{
    private readonly Engine engine;
    private RenderPipeline* renderPipeline;

    // Transform
    private Matrix4X4<float> transform = Matrix4X4<float>.Identity;
    private UniformBuffer<Matrix4X4<float>> transformBuffer;
    private BindGroupLayout* transformBindGroupLayout;
    private BindGroup* transformBindGroup;

    public UnlitRenderPipeline(Engine engine)
    {
        this.engine = engine;
    }

    public Matrix4X4<float> Transform
    {
        get => transform;
        set
        {
            transform = value;
            transformBuffer.Write(value);
        }
    }

    public void Initialize()
    {
        // BIND GROUP LAYOUTS
        CreateBindGroupLayouts();

        // SHADER MODULE
        ShaderModule* shaderModule = WebGPUUtil.ShaderModule.Create(engine, "Shaders/unlit.wgsl");

        // LAYOUT
        PipelineLayoutDescriptor pipelineLayoutDescriptor = new PipelineLayoutDescriptor();

        BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[1];
        bindGroupLayouts[0] = transformBindGroupLayout;
        pipelineLayoutDescriptor.BindGroupLayouts = bindGroupLayouts;

        pipelineLayoutDescriptor.BindGroupLayouts = bindGroupLayouts;
        pipelineLayoutDescriptor.BindGroupLayoutCount = 1;

        PipelineLayout* layout = engine.WGPU.DeviceCreatePipelineLayout(engine.Device, pipelineLayoutDescriptor);

        // PIPELINE
        renderPipeline = WebGPUUtil.RenderPipeline.Create(engine, shaderModule, layout);

        // Dispose of shader module.
        engine.WGPU.ShaderModuleRelease(shaderModule);
        engine.WGPU.PipelineLayoutRelease(layout);

        CreateResources();
        CreateBindGroups();
    }

    private void CreateResources()
    {
        transformBuffer = new UniformBuffer<Matrix4X4<float>>(engine);
        transformBuffer.Initialize(Transform);
    }


    private void CreateBindGroupLayouts()
    {
        // - TRANSFORM
        BindGroupLayoutEntry* bindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[1];

        // TODO: refactor.
        bindGroupLayoutEntries[0] = new BindGroupLayoutEntry();
        bindGroupLayoutEntries[0].Binding = 0;
        bindGroupLayoutEntries[0].Visibility = ShaderStage.Vertex;
        bindGroupLayoutEntries[0].Buffer = new BufferBindingLayout()
        {
            Type = BufferBindingType.Uniform
        };

        BindGroupLayoutDescriptor layoutDescriptor = new BindGroupLayoutDescriptor();
        layoutDescriptor.Entries = bindGroupLayoutEntries;
        layoutDescriptor.EntryCount = 1;

        transformBindGroupLayout = engine.WGPU.DeviceCreateBindGroupLayout(engine.Device, in layoutDescriptor);
    }

    private void CreateBindGroups()
    {
        // - TRANSFORM
        BindGroupEntry* bindGroupEntries = stackalloc BindGroupEntry[1];

        bindGroupEntries[0] = new BindGroupEntry();
        bindGroupEntries[0].Binding = 0;
        bindGroupEntries[0].Buffer = transformBuffer.Buffer;
        bindGroupEntries[0].Size = transformBuffer.Size;    

        BindGroupDescriptor descriptor = new BindGroupDescriptor();
        descriptor.Label = (byte*) Marshal.StringToHGlobalAnsi("Transform bind group.");
        descriptor.Layout = transformBindGroupLayout;
        descriptor.Entries = bindGroupEntries;
        descriptor.EntryCount = 1;

        transformBindGroup = engine.WGPU.DeviceCreateBindGroup(engine.Device, descriptor);
    }

    public void Render(VertexBuffer vertexBuffer, IndexBuffer? indexBuffer = null)
    {
        engine.WGPU.RenderPassEncoderSetPipeline(engine.CurrentRenderPassEncoder, renderPipeline);

        engine.WGPU.RenderPassEncoderSetBindGroup(engine.CurrentRenderPassEncoder, 0, transformBindGroup, 0, 0);

        // Set buffers.
        engine.WGPU.RenderPassEncoderSetVertexBuffer(
            engine.CurrentRenderPassEncoder, 
            0, 
            vertexBuffer.Buffer,
            0,
            vertexBuffer.Size);

        if (indexBuffer != null)
        {
            engine.WGPU.RenderPassEncoderSetIndexBuffer(
                engine.CurrentRenderPassEncoder, 
                indexBuffer.Buffer,
                IndexFormat.Uint16,
                0,
                indexBuffer.Size
            );
            
            engine.WGPU.RenderPassEncoderDrawIndexed(
                engine.CurrentRenderPassEncoder,
                indexBuffer.IndicesCount,
                1, 
                0, 0, 0);
        }
        else
        {
            engine.WGPU.RenderPassEncoderDraw(
                engine.CurrentRenderPassEncoder,
                vertexBuffer.VertexCount,
                1, 0, 0);
        }
    }

    public void Dispose()
    {
        engine.WGPU.RenderPipelineRelease(renderPipeline);
        engine.WGPU.BindGroupLayoutRelease(transformBindGroupLayout);
    }
}