using FPSGame.Buffers;
using FPSGame.Texture;
using FPSGame;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using System.Runtime.InteropServices;
using FPSGame.Camera;
using FPSGame.Extensions;
using FPSGame.Utils;

namespace WebGPU_FPS_Game.Pipelines
{
    internal unsafe class BillboardRenderPipeline
    {
        private readonly Engine engine;
        private RenderPipeline* renderPipeline;

        // Transform.
        private InstanceBuffer<Matrix4X4<float>> transformsBuffer;

        // Camera
        private readonly ICamera camera;
        private BindGroupLayout* cameraBindGroupLayout;
        private BindGroup* cameraBindGroup;

        // Texture
        private BindGroupLayout* textureBindGroupLayout;
        private BindGroup* textureBindGroup;
        private Texture2D defaultTexture = null!;
        private Texture2D texture = null!;

        public BillboardRenderPipeline(Engine engine,
            InstanceBuffer<Matrix4X4<float>> transformsBuffer,
            ICamera camera, string label = "")
        {
            this.engine = engine;
            this.transformsBuffer = transformsBuffer;
            this.camera = camera;
            Label = label;
        }

        public string Label { get; }

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
            defaultTexture = Texture2D.CreateEmptyTexture(engine, "Billboard Pipeline Default Texture");
            texture = defaultTexture;
        }

        private void CreateBindGroupLayouts()
        {
            // Camera
            BindGroupLayoutEntry* cameraBindGroupLayoutEntries = stackalloc BindGroupLayoutEntry[2];
            cameraBindGroupLayoutEntries[0] = new BindGroupLayoutEntry();
            cameraBindGroupLayoutEntries[0].Binding = 0;
            cameraBindGroupLayoutEntries[0].Visibility = ShaderStage.Vertex;
            cameraBindGroupLayoutEntries[0].Buffer = new BufferBindingLayout()
            {
                Type = BufferBindingType.Uniform
            };
            cameraBindGroupLayoutEntries[1] = new BindGroupLayoutEntry();
            cameraBindGroupLayoutEntries[1].Binding = 1;
            cameraBindGroupLayoutEntries[1].Visibility = ShaderStage.Vertex;
            cameraBindGroupLayoutEntries[1].Buffer = new BufferBindingLayout()
            {
                Type = BufferBindingType.Uniform
            };

            BindGroupLayoutDescriptor cameraBindGroupLayoutDesc = new BindGroupLayoutDescriptor();
            cameraBindGroupLayoutDesc.Label = "Billboard Render Pipeline Camera Bind Group Layout".ToBytePtr();
            cameraBindGroupLayoutDesc.Entries = cameraBindGroupLayoutEntries;
            cameraBindGroupLayoutDesc.EntryCount = 2;

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
            BindGroupEntry* cameraBindGroupEntries = stackalloc BindGroupEntry[2];

            cameraBindGroupEntries[0] = new BindGroupEntry();
            cameraBindGroupEntries[0].Binding = 0;
            cameraBindGroupEntries[0].Buffer = camera.ProjectionBuffer.Buffer;
            cameraBindGroupEntries[0].Size = camera.ProjectionBuffer.Size;

            cameraBindGroupEntries[1] = new BindGroupEntry();
            cameraBindGroupEntries[1].Binding = 1;
            cameraBindGroupEntries[1].Buffer = camera.ViewBuffer.Buffer;
            cameraBindGroupEntries[1].Size = camera.ViewBuffer.Size;

            BindGroupDescriptor cameraBindGroupDescriptor = new BindGroupDescriptor();
            cameraBindGroupDescriptor.Layout = cameraBindGroupLayout;
            cameraBindGroupDescriptor.Entries = cameraBindGroupEntries;
            cameraBindGroupDescriptor.EntryCount = 2;

            cameraBindGroup = engine.WGPU.DeviceCreateBindGroup(engine.Device, cameraBindGroupDescriptor);

            // - TEXTURE
            CreateTextureBindGroup();
        }

        public void Initialize()
        {
            // Layouts.
            CreateBindGroupLayouts();

            // Shader module.
            ShaderModule* shaderModule = WebGPUUtil.ShaderModule.Create(engine, "Shaders/billboard.wgsl", "Billboard Render Pipeline Shader Module");

            // Layout.
            PipelineLayoutDescriptor pipelineLayoutDescriptor = new PipelineLayoutDescriptor();

            BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[2];
            bindGroupLayouts[0] = cameraBindGroupLayout;
            bindGroupLayouts[1] = textureBindGroupLayout;
            pipelineLayoutDescriptor.BindGroupLayouts = bindGroupLayouts;
            pipelineLayoutDescriptor.BindGroupLayoutCount = 2;

            PipelineLayout* pipelineLayout =
                engine.WGPU.DeviceCreatePipelineLayout(engine.Device, pipelineLayoutDescriptor);

            // Vertex buffer layout.
            VertexBufferLayout[] vertexBufferLayouts = new VertexBufferLayout[2];

            VertexAttribute* vertexAttributes = stackalloc VertexAttribute[3];
            // Position
            vertexAttributes[0] = new VertexAttribute()
            {
                Format = VertexFormat.Float32x3, // xyz
                ShaderLocation = 0,
                Offset = 0
            };
            // Color
            vertexAttributes[1] = new VertexAttribute()
            {
                Format = VertexFormat.Float32x4, // rgba
                ShaderLocation = 1,
                Offset = sizeof(float) * 3
            };
            // Texture coords
            vertexAttributes[2] = new VertexAttribute()
            {
                Format = VertexFormat.Float32x2, // uv
                ShaderLocation = 2,
                Offset = sizeof(float) * (3 + 4)
            };

            vertexBufferLayouts[0] = new VertexBufferLayout()
            {
                StepMode = VertexStepMode.Vertex,
                Attributes = vertexAttributes,
                AttributeCount = 3,
                ArrayStride = 9 * sizeof(float)
            };

            // Instance buffer layout.
            VertexAttribute* instanceAttributes = stackalloc VertexAttribute[4];
            for (int i = 0; i < 4; i++)
            {
                instanceAttributes[i] = new VertexAttribute()
                {
                    Format = VertexFormat.Float32x4, // vec4 
                    ShaderLocation = (uint)(3 + i),
                    Offset = (uint)(i * sizeof(float) * 4)
                };
            }

            vertexBufferLayouts[1] = new VertexBufferLayout()
            {
                StepMode = VertexStepMode.Instance,
                Attributes = instanceAttributes,
                AttributeCount = 4,
                ArrayStride = 16 * sizeof(float)
            };

            renderPipeline = WebGPUUtil.RenderPipeline.Create(engine, shaderModule,
                vertexBufferLayouts, pipelineLayout, label: Label);

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
            engine.WGPU.RenderPassEncoderSetVertexBuffer(
                engine.CurrentRenderPassEncoder,
                1,
                transformsBuffer.Buffer,
                0,
                transformsBuffer.Size);

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
                    transformsBuffer.InstanceCount,
                    0, 0, 0);
            }
            else
            {
                engine.WGPU.RenderPassEncoderDraw(
                    engine.CurrentRenderPassEncoder,
                    vertexBuffer.VertexCount,
                    transformsBuffer.InstanceCount,
                    0, 0);
            }
        }

        public void Dispose()
        {
            engine.WGPU.RenderPipelineRelease(renderPipeline);

            // Release layouts
            engine.WGPU.BindGroupLayoutRelease(textureBindGroupLayout);

            // Release bind groups
            engine.WGPU.BindGroupRelease(textureBindGroup);
        }
    }
}
