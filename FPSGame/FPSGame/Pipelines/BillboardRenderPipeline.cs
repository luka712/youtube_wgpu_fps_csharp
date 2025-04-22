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
        private InstanceBuffer<Matrix4X4<float>> transformBuffer;

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
            InstanceBuffer<Matrix4X4<float>> transformBuffer,
            ICamera camera,
            string label = "")
        {
            this.engine = engine;
            this.camera = camera;
            this.transformBuffer = transformBuffer;
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

            VertexBufferLayout[] vertexBufferLayout = new VertexBufferLayout[2];

            // VERTEX ATTRUBUTES
            VertexAttribute* vertexAttributes = stackalloc VertexAttribute[3];
            // Vertex position
            vertexAttributes[0].Format = VertexFormat.Float32x3; // (xyz)
            vertexAttributes[0].ShaderLocation = 0;
            vertexAttributes[0].Offset = 0;
            // Vertex color
            vertexAttributes[1].Format = VertexFormat.Float32x4; // (rgba)
            vertexAttributes[1].ShaderLocation = 1;
            vertexAttributes[1].Offset = 3 * sizeof(float);

            // Vertex texture coords
            vertexAttributes[2].Format = VertexFormat.Float32x2; // (uv)
            vertexAttributes[2].ShaderLocation = 2;
            vertexAttributes[2].Offset = 7 * sizeof(float);

            vertexBufferLayout[0].StepMode = VertexStepMode.Vertex;
            vertexBufferLayout[0].Attributes = vertexAttributes;
            vertexBufferLayout[0].AttributeCount = 3;
            vertexBufferLayout[0].ArrayStride = 9 * sizeof(float);

            // INSTANCED ATTRIBUTES ( TRANSFORM )
            VertexAttribute* instanceAttributes = stackalloc VertexAttribute[4];
            // Row1
            instanceAttributes[0].Format = VertexFormat.Float32x4; // (xyzw)
            instanceAttributes[0].ShaderLocation = 3;
            instanceAttributes[0].Offset = 0 * sizeof(float);
            // Row2
            instanceAttributes[1].Format = VertexFormat.Float32x4; // (xyzw)
            instanceAttributes[1].ShaderLocation = 4;
            instanceAttributes[1].Offset = 4 * sizeof(float);
            // Row3
            instanceAttributes[2].Format = VertexFormat.Float32x4; // (xyzw)
            instanceAttributes[2].ShaderLocation = 5;
            instanceAttributes[2].Offset = 8 * sizeof(float);
            // Row4
            instanceAttributes[3].Format = VertexFormat.Float32x4; // (xyzw)
            instanceAttributes[3].ShaderLocation = 6;
            instanceAttributes[3].Offset = 12 * sizeof(float);

            vertexBufferLayout[1].StepMode = VertexStepMode.Instance;
            vertexBufferLayout[1].Attributes = instanceAttributes;
            vertexBufferLayout[1].AttributeCount = 4;
            vertexBufferLayout[1].ArrayStride = 4 * 4 * sizeof(float); // 4x4 matrix


            renderPipeline = WebGPUUtil.RenderPipeline.Create(engine,
                shaderModule,
                vertexBufferLayout,
                pipelineLayout, label: Label);

            // Resources.
            CreateResources();

            // Bind groups for resources.
            CreateBindGroups();

            // Dispose of shader module.
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
               transformBuffer.Buffer,
               0,
               transformBuffer.Size);

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
                    transformBuffer.InstanceCount,
                    0, 0, 0);
            }
            else
            {
                engine.WGPU.RenderPassEncoderDraw(
                    engine.CurrentRenderPassEncoder,
                    vertexBuffer.VertexCount,
                    transformBuffer.InstanceCount, 0, 0);
            }
        }

        public void Dispose()
        {
            transformBuffer.Dispose();
            engine.WGPU.RenderPipelineRelease(renderPipeline);

            // Release layouts
            engine.WGPU.BindGroupLayoutRelease(textureBindGroupLayout);

            // Release bind groups
            engine.WGPU.BindGroupRelease(textureBindGroup);
        }
    }
}
