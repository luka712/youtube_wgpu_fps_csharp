using FPSGame.Buffers;
using FPSGame.Utils;
using Silk.NET.Maths;
using Silk.NET.WebGPU;

namespace FPSGame.Pipelines;

public unsafe class UnlitRenderPipeline : IDisposable
{
    private readonly Engine engine;
    private RenderPipeline* renderPipeline;

    // Transform.
    private Matrix4X4<float> transform = Matrix4X4<float>.Identity;
    private UniformBuffer<Matrix4X4<float>> transformBuffer;
    private BindGroupLayout* transformBindGroupLayout; // Layout and description of data.
    private BindGroup* transformBindGroup; // Actual data.
   


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
            transformBuffer.Update(transform);
        }
    }

    private void CreateResources()
    {
        transformBuffer = new UniformBuffer<Matrix4X4<float>>(engine);
        transformBuffer.Initialize(transform);
    }

    private void CreateBindGroupLayouts()
    {
        BindGroupLayoutEntry* bindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[1];
        bindGroupLayoutEntries[0] = new BindGroupLayoutEntry();
        bindGroupLayoutEntries[0].Binding = 0;
        bindGroupLayoutEntries[0].Visibility = ShaderStage.Vertex;
        bindGroupLayoutEntries[0].Buffer = new BufferBindingLayout()
        {
            Type = BufferBindingType.Uniform
        };

        BindGroupLayoutDescriptor descriptor = new BindGroupLayoutDescriptor();
        descriptor.Entries = bindGroupLayoutEntries;
        descriptor.EntryCount = 1;

        transformBindGroupLayout = engine.WGPU.DeviceCreateBindGroupLayout(engine.Device, descriptor);
    }

    private void CreateBindGroups()
    {
        BindGroupEntry* bindGroupEntries = stackalloc BindGroupEntry[1];

        bindGroupEntries[0] = new BindGroupEntry();
        bindGroupEntries[0].Binding = 0;
        bindGroupEntries[0].Buffer = transformBuffer.Buffer;
        bindGroupEntries[0].Size = transformBuffer.Size;

        BindGroupDescriptor descriptor = new BindGroupDescriptor();
        descriptor.Layout = transformBindGroupLayout;
        descriptor.Entries = bindGroupEntries;
        descriptor.EntryCount = 1;

        transformBindGroup = engine.WGPU.DeviceCreateBindGroup(engine.Device, descriptor);
    }

    public void Initialize()
    {
        // Layouts.
        CreateBindGroupLayouts();

        // Shader module.
        ShaderModule* shaderModule = WebGPUUtil.ShaderModule.Create(engine, "Shaders/unlit.wgsl");

        // Layout.
        PipelineLayoutDescriptor pipelineLayoutDescriptor  = new PipelineLayoutDescriptor();

        BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[1];
        bindGroupLayouts[0] = transformBindGroupLayout;
        pipelineLayoutDescriptor.BindGroupLayouts = bindGroupLayouts;
        pipelineLayoutDescriptor.BindGroupLayoutCount = 1;

        PipelineLayout* pipelineLayout = engine.WGPU.DeviceCreatePipelineLayout(engine.Device, pipelineLayoutDescriptor);

        renderPipeline = WebGPUUtil.RenderPipeline.Create(engine, shaderModule, pipelineLayout);

        // Resources.
        CreateResources();

        // Bind groups for resources.
        CreateBindGroups();

        // DIspose of shader module.
        engine.WGPU.ShaderModuleRelease(shaderModule);
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
        transformBuffer.Dispose();
        engine.WGPU.RenderPipelineRelease(renderPipeline);
    }
}