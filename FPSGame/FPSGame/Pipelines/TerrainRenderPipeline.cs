using System.Runtime.InteropServices;
using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Extensions;
using FPSGame.Texture;
using FPSGame.Utils;
using Silk.NET.Maths;
using Silk.NET.WebGPU;

namespace FPSGame.Pipelines;

public unsafe class TerrainRenderPipeline : IDisposable
{
    private readonly Engine engine;
    private RenderPipeline* renderPipeline;

    // Transform.
    private Matrix4X4<float> transform = Matrix4X4<float>.Identity;
    private UniformBuffer<Matrix4X4<float>> transformBuffer;
    private UniformBuffer<Vector2D<float>> textureTillingBuffer;
    private BindGroupLayout* transformBindGroupLayout; // Layout and description of data.
    private BindGroup* transformBindGroup; // Actual data.
    
    // Camera
    private readonly ICamera camera;
    private BindGroupLayout* cameraBindGroupLayout;
    private BindGroup* cameraBindGroup;

    // Texture
    private BindGroupLayout* fragmentBindGroupLayout;
    private BindGroup* fragmentBindGroup;
    private Texture2D defaultTexture = null!;
    private Texture2D mixTexture = null!;
    private Texture2D redTexture = null!;
    private Texture2D greenTexture = null!;
    private Texture2D blueTexture = null!;
    private Vector2D<float> textureTilling = new(1, 1);

    public TerrainRenderPipeline(Engine engine, ICamera camera, string label = "")
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

    public Texture2D? MixTexture
    {
        get => mixTexture;
        set
        {
            mixTexture = value ?? defaultTexture;
            CreateTextureBindGroup();
        }
    }
    
    public Texture2D? RedTexture
    {
        get => redTexture;
        set
        {
            redTexture = value ?? defaultTexture;
            CreateTextureBindGroup();
        }
    }
    
    public Texture2D? GreenTexture
    {
        get => greenTexture;
        set
        {
            greenTexture = value ?? defaultTexture;
            CreateTextureBindGroup();
        }
    }
    
    public Texture2D? BlueTexture
    {
        get => blueTexture;
        set
        {
            blueTexture = value ?? defaultTexture;
            CreateTextureBindGroup();
        }
    }

    public Vector2D<float> TextureTilling
    {
        get => textureTilling;
        set
        {
            textureTilling = value;
            textureTillingBuffer.Update(textureTilling);
        }
    }

    private void CreateResources()
    {
        transformBuffer = new UniformBuffer<Matrix4X4<float>>(engine, "Terrain Render Pipeline Transform Buffer");
        transformBuffer.Initialize(transform);
        
        textureTillingBuffer = new UniformBuffer<Vector2D<float>>(engine, "Terrain Render Pipeline Texture Tilling Buffer");
        textureTillingBuffer.Initialize(textureTilling);

        defaultTexture = Texture2D.CreateEmptyTexture(engine, "Terrain Pipeline Default Texture");
        mixTexture = defaultTexture;
        redTexture = defaultTexture;
        greenTexture = defaultTexture;
        blueTexture = defaultTexture;
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
        
        // Fragment
        BindGroupLayoutEntry* fragmentBindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[9];

        // Mix texture, Mix Sampler
        // Red texture, Red Sampler
        // Green texture, Green Sampler
        // Blue texture, Blue Sampler
        for (int i = 0; i < 8; i += 2)
        {
            // Texture
            fragmentBindGroupLayoutEntries[i] = new();
            fragmentBindGroupLayoutEntries[i].Binding = (uint) i;
            fragmentBindGroupLayoutEntries[i].Visibility = ShaderStage.Fragment;
            fragmentBindGroupLayoutEntries[i].Texture = new()
            {
                SampleType = TextureSampleType.Float,
                Multisampled = false,
                ViewDimension = TextureViewDimension.Dimension2D
            };

            // Sampler
            fragmentBindGroupLayoutEntries[i + 1] = new();
            fragmentBindGroupLayoutEntries[i + 1].Binding = (uint) i + 1;
            fragmentBindGroupLayoutEntries[i + 1].Visibility = ShaderStage.Fragment;
            fragmentBindGroupLayoutEntries[i + 1].Sampler = new()
            {
                Type = SamplerBindingType.Filtering
            };
        }
        
        // Texture Tilling
        fragmentBindGroupLayoutEntries[8] = new();
        fragmentBindGroupLayoutEntries[8].Binding = 8;
        fragmentBindGroupLayoutEntries[8].Visibility = ShaderStage.Fragment;
        fragmentBindGroupLayoutEntries[8].Buffer = new BufferBindingLayout()
        {
            Type = BufferBindingType.Uniform
        };

        BindGroupLayoutDescriptor fragmentBindGroupLayoutDesc = new BindGroupLayoutDescriptor();
        fragmentBindGroupLayoutDesc.Entries = fragmentBindGroupLayoutEntries;
        fragmentBindGroupLayoutDesc.EntryCount = 9;

        fragmentBindGroupLayout = engine.WGPU.DeviceCreateBindGroupLayout(engine.Device, fragmentBindGroupLayoutDesc);
    }

    private void CreateTextureBindGroup()
    {
        if (fragmentBindGroup != null)
        {
            engine.WGPU.BindGroupRelease(fragmentBindGroup);
            fragmentBindGroup = null;
        }
        
        // - TEXTURE
        BindGroupEntry* textureBindGroupEntries = stackalloc BindGroupEntry[9];

        Texture2D[] textures = [mixTexture, redTexture, greenTexture, blueTexture];

        int texIndex = 0;
        for (int i = 0; i < 8; i += 2)
        {
            textureBindGroupEntries[i] = new()
            {
                Binding = (uint) i,
                TextureView = textures[texIndex].TextureView,
            };
            textureBindGroupEntries[i+1] = new()
            {
                Binding = (uint) i+1,
                Sampler = textures[texIndex].Sampler,
            };

            texIndex++;
        }
        textureBindGroupEntries[8] = new()
        {
            Binding = 8,
            Buffer = textureTillingBuffer.Buffer,
            Size = textureTillingBuffer.Size
        };
        

        BindGroupDescriptor desc = new()
        {
            Label = (byte*)Marshal.StringToHGlobalAnsi("Unlit Render Pipeline Texture Bind Group"),
            Layout = fragmentBindGroupLayout,
            Entries = textureBindGroupEntries,
            EntryCount = 9
        };



        fragmentBindGroup = engine.WGPU.DeviceCreateBindGroup(engine.Device, desc);
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
        ShaderModule* shaderModule = WebGPUUtil.ShaderModule.Create(engine, "Shaders/terrain.wgsl", "Terrain Render Pipeline Shader Module");

        // Layout.
        PipelineLayoutDescriptor pipelineLayoutDescriptor = new PipelineLayoutDescriptor();

        BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[3];
        bindGroupLayouts[0] = transformBindGroupLayout;
        bindGroupLayouts[1] = cameraBindGroupLayout; 
        bindGroupLayouts[2] = fragmentBindGroupLayout;
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
            fragmentBindGroup,
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
        engine.WGPU.BindGroupLayoutRelease(fragmentBindGroupLayout);

        // Release bind groups
        engine.WGPU.BindGroupRelease(transformBindGroup);
        engine.WGPU.BindGroupRelease(fragmentBindGroup);
    }
}