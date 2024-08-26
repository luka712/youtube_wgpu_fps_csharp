using System.Runtime.InteropServices;
using FPSGame.Buffers;
using FPSGame.Texture;
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
    
    // Texture
    private BindGroupLayout* textureBindGroupLayout;
    private BindGroup* textureBindGroup;
    private Texture2D defaultTexture = null!;
    private Texture2D texture = null!;

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

    public Texture2D? Texture
    {
        get => texture;
        set
        {
            texture = value ?? defaultTexture;
            CreateTextureBindGroup();
        }
    }

    private void CreateResources()
    {
        transformBuffer = new UniformBuffer<Matrix4X4<float>>(engine);
        transformBuffer.Initialize(transform);
        
        defaultTexture = Texture2D.CreateEmptyTexture(engine, "Unlit Pipeline Default Texture");
        texture = defaultTexture;
    }

    private void CreateBindGroupLayouts()
    {
        // Model
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
        
        // Texture
        BindGroupLayoutEntry* textureBindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[2];
        textureBindGroupLayoutEntries[0] = new ();
        textureBindGroupLayoutEntries[0].Binding = 0;
        textureBindGroupLayoutEntries[0].Visibility = ShaderStage.Fragment;
        textureBindGroupLayoutEntries[0].Texture = new()
        {
            SampleType = TextureSampleType.Float,
            Multisampled = false,
            ViewDimension =  TextureViewDimension.Dimension2D
        };
        
        textureBindGroupLayoutEntries[1] = new ();
        textureBindGroupLayoutEntries[1].Binding = 1;
        textureBindGroupLayoutEntries[1].Visibility = ShaderStage.Fragment;
        textureBindGroupLayoutEntries[1].Sampler = new()
        {
            Type = SamplerBindingType.Filtering
        };

        descriptor = new ();
        descriptor.Entries = textureBindGroupLayoutEntries;
        descriptor.EntryCount = 2;

        textureBindGroupLayout = engine.WGPU.DeviceCreateBindGroupLayout(engine.Device, descriptor);
    }

    private void CreateTextureBindGroup()
    {
        
        // - TEXTURE
        BindGroupEntry* textureBindGroupEntries = stackalloc BindGroupEntry[2];

        textureBindGroupEntries[0] = new()
        {
            Binding =  0,
            TextureView = texture.TextureView,
        };
        textureBindGroupEntries[1] = new()
        {
            Binding =  1,
            Sampler = texture.Sampler,
        };

        BindGroupDescriptor desc = new()
        {
            Label = (byte*) Marshal.StringToHGlobalAnsi("Unlit Rendere Pipeline Texture Bind Group"),
            Layout = textureBindGroupLayout,
            Entries = textureBindGroupEntries,
            EntryCount = 2
        };

        if (textureBindGroup != null)
        {
            engine.WGPU.BindGroupRelease(textureBindGroup);
            textureBindGroup = null;
        }

        textureBindGroup = engine.WGPU.DeviceCreateBindGroup(engine.Device, desc);
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
        descriptor.Layout = transformBindGroupLayout;
        descriptor.Entries = bindGroupEntries;
        descriptor.EntryCount = 1;

        transformBindGroup = engine.WGPU.DeviceCreateBindGroup(engine.Device, descriptor);
    
        // - TEXTURE
        CreateTextureBindGroup();
    }

    public void Initialize()
    {
        // Layouts.
        CreateBindGroupLayouts();

        // Shader module.
        ShaderModule* shaderModule = WebGPUUtil.ShaderModule.Create(engine, "Shaders/unlit.wgsl");

        // Layout.
        PipelineLayoutDescriptor pipelineLayoutDescriptor = new PipelineLayoutDescriptor();

        BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[2];
        bindGroupLayouts[0] = transformBindGroupLayout;
        bindGroupLayouts[1] = textureBindGroupLayout;
        pipelineLayoutDescriptor.BindGroupLayouts = bindGroupLayouts;
        pipelineLayoutDescriptor.BindGroupLayoutCount = 2;

        PipelineLayout* pipelineLayout =
            engine.WGPU.DeviceCreatePipelineLayout(engine.Device, pipelineLayoutDescriptor);

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

        engine.WGPU.RenderPassEncoderSetBindGroup(engine.CurrentRenderPassEncoder, 
            0, 
            transformBindGroup, 
            0, 
            0);
        engine.WGPU.RenderPassEncoderSetBindGroup(engine.CurrentRenderPassEncoder, 
            1, 
            textureBindGroup, 
            0, 
            0);

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
        
        // Release layouts
        engine.WGPU.BindGroupLayoutRelease(transformBindGroupLayout);
        engine.WGPU.BindGroupLayoutRelease(textureBindGroupLayout);
        
        // Release bind groups
        engine.WGPU.BindGroupRelease(transformBindGroup);
        engine.WGPU.BindGroupRelease(textureBindGroup);
    }
}