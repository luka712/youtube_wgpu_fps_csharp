using System.Runtime.InteropServices;
using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Texture;
using Silk.NET.WebGPU;
using FPSGame.Extensions;
using FPSGame.Utils;

namespace FPSGame.Pipelines;

public unsafe class SkyboxRenderPipeline : IDisposable
{
    private readonly Engine engine;
    private RenderPipeline* renderPipeline;

    private VertexBuffer vertexBuffer;

    // Camera
    private readonly ICamera camera;
    private BindGroupLayout* cameraBindGroupLayout;
    private BindGroup* cameraBindGroup;

    // Texture
    private BindGroupLayout* textureBindGroupLayout;
    private BindGroup* textureBindGroup;
    private CubeTexture cubeTexture = null!;

    public SkyboxRenderPipeline(Engine engine, CubeTexture cubeTexture, ICamera camera, string label = "")
    {
        this.engine = engine;
        this.camera = camera;
        Label = label;
        this.cubeTexture = cubeTexture;
    }

    public string Label { get; }

    public CubeTexture Texture
    {
        get => cubeTexture;
        set
        {
            cubeTexture = value;
            CreateTextureBindGroup();
        }
    }

    private void CreateResources()
    {
        vertexBuffer = new VertexBuffer(engine);
        Geometry geometry = GeometryBuilder.CreateSkyboxGeometry();
        vertexBuffer.Initialize(geometry.InterleavedVertices, geometry.VertexCount);
    }

    private void CreateBindGroupLayouts()
    {
        // Camera
        BindGroupLayoutEntry* cameraBindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[1];
        cameraBindGroupLayoutEntries[0] = new BindGroupLayoutEntry();
        cameraBindGroupLayoutEntries[0].Binding = 0;
        cameraBindGroupLayoutEntries[0].Visibility = ShaderStage.Vertex;
        cameraBindGroupLayoutEntries[0].Buffer = new BufferBindingLayout()
        {
            Type = BufferBindingType.Uniform
        };

        BindGroupLayoutDescriptor cameraBindGroupLayoutDesc = new();
        cameraBindGroupLayoutDesc.Label = "Skybox Render Pipeline Camera Bind Group Layout".ToBytePtr();
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
            ViewDimension = TextureViewDimension.DimensionCube
        };

        textureBindGroupLayoutEntries[1] = new();
        textureBindGroupLayoutEntries[1].Binding = 1;
        textureBindGroupLayoutEntries[1].Visibility = ShaderStage.Fragment;
        textureBindGroupLayoutEntries[1].Sampler = new()
        {
            Type = SamplerBindingType.Filtering
        };

        BindGroupLayoutDescriptor textureBindGroupLayoutDesc = new();
        textureBindGroupLayoutDesc.Entries = textureBindGroupLayoutEntries;
        textureBindGroupLayoutDesc.EntryCount = 2;

        textureBindGroupLayout = engine.WGPU.DeviceCreateBindGroupLayout(engine.Device, textureBindGroupLayoutDesc);
    }

    private void CreateTextureBindGroup()
    {
        // - TEXTURE
        BindGroupEntry* textureBindGroupEntries = stackalloc BindGroupEntry[2];

        textureBindGroupEntries[0] = new()
        {
            Binding = 0,
            TextureView = cubeTexture.TextureView,
        };
        textureBindGroupEntries[1] = new()
        {
            Binding = 1,
            Sampler = cubeTexture.Sampler,
        };

        BindGroupDescriptor desc = new()
        {
            Label = (byte*)Marshal.StringToHGlobalAnsi("Skybox Render Pipeline Texture Bind Group"),
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
        // - CAMERA
        BindGroupEntry* cameraBindGroupEntries = stackalloc BindGroupEntry[1];

        cameraBindGroupEntries[0] = new BindGroupEntry();
        cameraBindGroupEntries[0].Binding = 0;
        cameraBindGroupEntries[0].Buffer = camera.SkyboxProjectionViewBuffer.Buffer;
        cameraBindGroupEntries[0].Size = camera.SkyboxProjectionViewBuffer.Size;

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
        ShaderModule* shaderModule =
            WebGPUUtil.ShaderModule.Create(engine, "Shaders/skybox.wgsl", "Skybox Render Pipeline Shader Module");

        // Layout.
        PipelineLayoutDescriptor pipelineLayoutDescriptor = new PipelineLayoutDescriptor();

        BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[2];
        bindGroupLayouts[0] = cameraBindGroupLayout;
        bindGroupLayouts[1] = textureBindGroupLayout;
        pipelineLayoutDescriptor.BindGroupLayouts = bindGroupLayouts;
        pipelineLayoutDescriptor.BindGroupLayoutCount = 2;

        PipelineLayout* pipelineLayout =
            engine.WGPU.DeviceCreatePipelineLayout(engine.Device, pipelineLayoutDescriptor);

        VertexAttribute* vertexAttributes = stackalloc VertexAttribute[1];

        // Vertex position
        vertexAttributes[0].Format = VertexFormat.Float32x3;
        vertexAttributes[0].ShaderLocation = 0;
        vertexAttributes[0].Offset = 0;

        VertexBufferLayout vertexBufferLayout = new();
        vertexBufferLayout.StepMode = VertexStepMode.Vertex;
        vertexBufferLayout.Attributes = vertexAttributes;
        vertexBufferLayout.AttributeCount = 1;
        vertexBufferLayout.ArrayStride = 3 * sizeof(float);

        renderPipeline =
            WebGPUUtil.RenderPipeline.Create(engine, shaderModule, &vertexBufferLayout, pipelineLayout, label: Label);

        // Resources.
        CreateResources();

        // Bind groups for resources.
        CreateBindGroups();

        // DIspose of shader module.
        engine.WGPU.ShaderModuleRelease(shaderModule);
    }

    public void Render()
    {
        engine.WGPU.RenderPassEncoderSetPipeline(engine.CurrentRenderPassEncoder, renderPipeline);

        engine.WGPU.RenderPassEncoderSetBindGroup(engine.CurrentRenderPassEncoder,
            0,
            cameraBindGroup,
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

        engine.WGPU.RenderPassEncoderDraw(
            engine.CurrentRenderPassEncoder,
            vertexBuffer.VertexCount,
            1, 0, 0);
    }

    public void Dispose()
    {
        vertexBuffer.Dispose();
        engine.WGPU.RenderPipelineRelease(renderPipeline);

        // Release layouts
        engine.WGPU.BindGroupLayoutRelease(textureBindGroupLayout);
        engine.WGPU.BindGroupLayoutRelease(cameraBindGroupLayout);

        // Release bind groups
        engine.WGPU.BindGroupRelease(textureBindGroup);
        engine.WGPU.BindGroupRelease(cameraBindGroup);
    }
}