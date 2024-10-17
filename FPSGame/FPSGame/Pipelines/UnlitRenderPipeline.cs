using System.Runtime.InteropServices;
using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Extensions;
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
    
    // Camera
    private readonly PerspectiveCamera camera;
    private BindGroupLayout* cameraBindGroupLayout;
    private BindGroup* cameraBindGroup;

    // Texture
    private BindGroupLayout* textureBindGroupLayout;
    private BindGroup* textureBindGroup;
    private Texture2D defaultTexture = null!;
    private Texture2D texture = null!;

    public UnlitRenderPipeline(Engine engine, PerspectiveCamera camera, string label = "")
    {
        this.engine = engine;
        this.camera = camera;
        Label = label;
    }

    public string Label { get;  }

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
        transformBuffer = new UniformBuffer<Matrix4X4<float>>(engine, "Unlit Render Pipeline Transform Buffer");
        transformBuffer.Initialize(transform);

        defaultTexture = Texture2D.CreateEmptyTexture(engine, "Unlit Pipeline Default Texture");
        texture = defaultTexture;
    }

    private void CreateBindGroupLayouts()
    {
        // Model
        BindGroupLayoutEntry* modelBindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[1];
        modelBindGroupLayoutEntries[0] = new BindGroupLayoutEntry();
        modelBindGroupLayoutEntries[0].Binding = 0;
        modelBindGroupLayoutEntries[0].Visibility = ShaderStage.Vertex;
        modelBindGroupLayoutEntries[0].Buffer = new BufferBindingLayout()
        {
            Type = BufferBindingType.Uniform
        };

        BindGroupLayoutDescriptor modelBindGroupLayoutDesc = new BindGroupLayoutDescriptor();
        modelBindGroupLayoutDesc.Label = "Unlit Render Pipeline Transform Bind Group Layout".ToBytePtr();
        modelBindGroupLayoutDesc.Entries = modelBindGroupLayoutEntries;
        modelBindGroupLayoutDesc.EntryCount = 1;

        transformBindGroupLayout = engine.WGPU.DeviceCreateBindGroupLayout(engine.Device, modelBindGroupLayoutDesc);

        // Camera
        BindGroupLayoutEntry* cameraBindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[1];
        cameraBindGroupLayoutEntries[0] = new BindGroupLayoutEntry();
        cameraBindGroupLayoutEntries[0].Binding = 0;
        cameraBindGroupLayoutEntries[0].Visibility = ShaderStage.Vertex;
        cameraBindGroupLayoutEntries[0].Buffer = new BufferBindingLayout()
        {
            Type = BufferBindingType.Uniform
        };

        BindGroupLayoutDescriptor cameraBindGroupLayoutDesc = new BindGroupLayoutDescriptor();
        cameraBindGroupLayoutDesc.Label = "Unlit Render Pipeline Camera Bind Group Layout".ToBytePtr();
        cameraBindGroupLayoutDesc.Entries = cameraBindGroupLayoutEntries;
        cameraBindGroupLayoutDesc.EntryCount = 1;

        cameraBindGroupLayout = engine.WGPU.DeviceCreateBindGroupLayout(engine.Device, cameraBindGroupLayoutDesc);
        
        // Texture
        BindGroupLayoutEntry* textureBindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[2];
        textureBindGroupLayoutEntries[0] = new();
        textureBindGroupLayoutEntries[0].Binding = 0;
        textureBindGroupLayoutEntries[0].Visibility = ShaderStage.Fragment;
        textureBindGroupLayoutEntries[0].Texture = new()
        {
            SampleType = TextureSampleType.Float,
            Multisampled = false,
            ViewDimension = TextureViewDimension.Dimension2D
        };

        textureBindGroupLayoutEntries[1] = new();
        textureBindGroupLayoutEntries[1].Binding = 1;
        textureBindGroupLayoutEntries[1].Visibility = ShaderStage.Fragment;
        textureBindGroupLayoutEntries[1].Sampler = new()
        {
            Type = SamplerBindingType.Filtering
        };

        modelBindGroupLayoutDesc = new();
        modelBindGroupLayoutDesc.Entries = textureBindGroupLayoutEntries;
        modelBindGroupLayoutDesc.EntryCount = 2;

        textureBindGroupLayout = engine.WGPU.DeviceCreateBindGroupLayout(engine.Device, modelBindGroupLayoutDesc);
    }

    private void CreateTextureBindGroup()
    {

        // - TEXTURE
        BindGroupEntry* textureBindGroupEntries = stackalloc BindGroupEntry[2];

        textureBindGroupEntries[0] = new()
        {
            Binding = 0,
            TextureView = texture.TextureView,
        };
        textureBindGroupEntries[1] = new()
        {
            Binding = 1,
            Sampler = texture.Sampler,
        };

        BindGroupDescriptor desc = new()
        {
            Label = (byte*)Marshal.StringToHGlobalAnsi("Unlit Render Pipeline Texture Bind Group"),
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
        BindGroupEntry* modelBindGroupEntries = stackalloc BindGroupEntry[1];

        modelBindGroupEntries[0] = new BindGroupEntry();
        modelBindGroupEntries[0].Binding = 0;
        modelBindGroupEntries[0].Buffer = transformBuffer.Buffer;
        modelBindGroupEntries[0].Size = transformBuffer.Size;

        BindGroupDescriptor modelBindGroupDescriptor = new BindGroupDescriptor();
        modelBindGroupDescriptor.Layout = transformBindGroupLayout;
        modelBindGroupDescriptor.Entries = modelBindGroupEntries;
        modelBindGroupDescriptor.EntryCount = 1;

        transformBindGroup = engine.WGPU.DeviceCreateBindGroup(engine.Device, modelBindGroupDescriptor);
        
        // - CAMERA
        BindGroupEntry* cameraBindGroupEntries = stackalloc BindGroupEntry[1];

        cameraBindGroupEntries[0] = new BindGroupEntry();
        cameraBindGroupEntries[0].Binding = 0;
        cameraBindGroupEntries[0].Buffer = camera.Buffer.Buffer;
        cameraBindGroupEntries[0].Size = camera.Buffer.Size;

        BindGroupDescriptor cameraBindGroupDescriptor = new BindGroupDescriptor();
        cameraBindGroupDescriptor.Layout = cameraBindGroupLayout;
        cameraBindGroupDescriptor.Entries = cameraBindGroupEntries;
        cameraBindGroupDescriptor.EntryCount = 1;

        cameraBindGroup = engine.WGPU.DeviceCreateBindGroup(engine.Device, cameraBindGroupDescriptor);

        // - TEXTURE
        CreateTextureBindGroup();
    }

    public void Initialize()
    {
        // Layouts.
        CreateBindGroupLayouts();

        // Shader module.
        ShaderModule* shaderModule = WebGPUUtil.ShaderModule.Create(engine, "Shaders/unlit.wgsl", "Unlit Render Pipeline Shader Module");

        // Layout.
        PipelineLayoutDescriptor pipelineLayoutDescriptor = new PipelineLayoutDescriptor();

        BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[3];
        bindGroupLayouts[0] = transformBindGroupLayout;
        bindGroupLayouts[1] = cameraBindGroupLayout; 
        bindGroupLayouts[2] = textureBindGroupLayout;
        pipelineLayoutDescriptor.BindGroupLayouts = bindGroupLayouts;
        pipelineLayoutDescriptor.BindGroupLayoutCount = 3;

        PipelineLayout* pipelineLayout =
            engine.WGPU.DeviceCreatePipelineLayout(engine.Device, pipelineLayoutDescriptor);

        renderPipeline = WebGPUUtil.RenderPipeline.Create(engine, shaderModule, pipelineLayout, label: Label);

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
            cameraBindGroup,
            0,
            0);
        engine.WGPU.RenderPassEncoderSetBindGroup(engine.CurrentRenderPassEncoder,
            2,
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