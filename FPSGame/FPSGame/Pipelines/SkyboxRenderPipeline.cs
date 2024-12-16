using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Extensions;
using FPSGame.Texture;
using FPSGame.Utils;
using Silk.NET.WebGPU;
using System.Runtime.InteropServices;

namespace FPSGame.Pipelines
{
    internal unsafe class SkyboxRenderPipeline : IDisposable
    {
        private readonly Engine engine;
        private RenderPipeline* renderPipeline;

        private VertexBuffer buffer;

        // Camera
        private readonly ICamera camera;
        private BindGroupLayout* cameraBindGroupLayout;
        private BindGroup* cameraBindGroup;

        // Texture
        private BindGroupLayout* textureBindGroupLayout;
        private BindGroup* textureBindGroup;
        private CubeTexture texture = null!;

        public SkyboxRenderPipeline(Engine engine, ICamera camera, CubeTexture skyboxTexture, string label = "")
        {
            this.engine = engine;
            this.camera = camera;
            this.texture = skyboxTexture;
            Label = label;
        }

        public string Label { get; }

        public CubeTexture Texture
        {
            get => texture;
            set
            {
                texture = value;
                CreateTextureBindGroup();
            }
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

            BindGroupLayoutDescriptor bindGroupLayoutDesc = new BindGroupLayoutDescriptor();
            bindGroupLayoutDesc.Label = "Skybox Render Pipeline Camera Bind Group Layout".ToBytePtr();
            bindGroupLayoutDesc.Entries = cameraBindGroupLayoutEntries;
            bindGroupLayoutDesc.EntryCount = 1;

            cameraBindGroupLayout = engine.WGPU.DeviceCreateBindGroupLayout(engine.Device, bindGroupLayoutDesc);

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
                Type = SamplerBindingType.Filtering,
            };

            bindGroupLayoutDesc.Label = "Skybox Render Pipeline Texture Bind Group Layout".ToBytePtr();
            bindGroupLayoutDesc.Entries = textureBindGroupLayoutEntries;
            bindGroupLayoutDesc.EntryCount = 2;

            textureBindGroupLayout = engine.WGPU.DeviceCreateBindGroupLayout(engine.Device, bindGroupLayoutDesc);
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

        private void CreateResources()
        {
            Geometry geometry = GeometryBuilder.CreateSkyboxGeometry();
            buffer = new VertexBuffer(engine);
            buffer.Initialize(geometry.InterleavedVertices, geometry.VertexCount);
        }

        public void Initialize()
        {
            CreateResources(); 

            // Layouts.
            CreateBindGroupLayouts();

            // Shader module.
            ShaderModule* shaderModule = WebGPUUtil.ShaderModule.Create(engine, "Shaders/skybox.wgsl", "Skybox Render Pipeline Shader Module");

            // Vertex layout
            VertexAttribute* vertexAttributes = stackalloc VertexAttribute[1];

            // Vertex position
            vertexAttributes[0].Format = VertexFormat.Float32x3;
            vertexAttributes[0].ShaderLocation = 0;
            vertexAttributes[0].Offset = 0;

            VertexBufferLayout vertexBufferLayout = new VertexBufferLayout();
            vertexBufferLayout.StepMode = VertexStepMode.Vertex;
            vertexBufferLayout.Attributes = vertexAttributes;
            vertexBufferLayout.AttributeCount = 1;
            vertexBufferLayout.ArrayStride = 3 * sizeof(float);

            // Layout.
            PipelineLayoutDescriptor pipelineLayoutDescriptor = new PipelineLayoutDescriptor();

            BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[2];
            bindGroupLayouts[0] = cameraBindGroupLayout;
            bindGroupLayouts[1] = textureBindGroupLayout;
            pipelineLayoutDescriptor.BindGroupLayouts = bindGroupLayouts;
            pipelineLayoutDescriptor.BindGroupLayoutCount = 2;

            PipelineLayout* pipelineLayout =
                engine.WGPU.DeviceCreatePipelineLayout(engine.Device, pipelineLayoutDescriptor);

            renderPipeline = WebGPUUtil.RenderPipeline.Create(engine, shaderModule, vertexBufferLayout, pipelineLayout,
                cullMode: CullMode.None, label: Label);

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
                buffer.Buffer,
                0,
                buffer.Size);


            engine.WGPU.RenderPassEncoderDraw(
                engine.CurrentRenderPassEncoder,
                buffer.VertexCount,
                1, 0, 0);

        }

        public void Dispose()
        {
            engine.WGPU.RenderPipelineRelease(renderPipeline);

            // Release layouts
            engine.WGPU.BindGroupLayoutRelease(cameraBindGroupLayout);
            engine.WGPU.BindGroupLayoutRelease(textureBindGroupLayout);

            // Release bind groups
            engine.WGPU.BindGroupRelease(cameraBindGroup);
            engine.WGPU.BindGroupRelease(textureBindGroup);
        }
    }
}
