using FPSGame.Buffers;
using FPSGame;
using Silk.NET.WebGPU;
using FPSGame.Camera;
using FPSGame.Extensions;
using FPSGame.Utils;

namespace WebGPU_FPS_Game.Pipelines
{
    public unsafe class WireframePipeline
    {
        private readonly Engine engine;
        private RenderPipeline* renderPipeline;

        // Camera
        private readonly ICamera camera;
        private BindGroupLayout* cameraBindGroupLayout;
        private BindGroup* cameraBindGroup;


        public WireframePipeline(Engine engine, ICamera camera, string label = "")
        {
            this.engine = engine;
            this.camera = camera;
            Label = label;
        }

        public string Label { get; }

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
        }

        private void CreateBindGroups()
        {
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

        }

        public void Initialize()
        {
            // Layouts.
            CreateBindGroupLayouts();

            // Shader module.
            ShaderModule* shaderModule =
                WebGPUUtil.ShaderModule.Create(engine, "Shaders/wireframe.wgsl", "Wireframe Render Pipeline Shader Module");

            // Layout.
            PipelineLayoutDescriptor pipelineLayoutDescriptor = new PipelineLayoutDescriptor();

            BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[1];
            bindGroupLayouts[0] = cameraBindGroupLayout;
            pipelineLayoutDescriptor.BindGroupLayouts = bindGroupLayouts;
            pipelineLayoutDescriptor.BindGroupLayoutCount = 1;

            PipelineLayout* pipelineLayout =
                engine.WGPU.DeviceCreatePipelineLayout(engine.Device, pipelineLayoutDescriptor);

            VertexAttribute* vertexAttributes = stackalloc VertexAttribute[2];

            // Vertex position
            vertexAttributes[0].Format = VertexFormat.Float32x3; // (xyz)
            vertexAttributes[0].ShaderLocation = 0;
            vertexAttributes[0].Offset = 0;

            // Color
            vertexAttributes[1].Format = VertexFormat.Float32x3; // (rgb)
            vertexAttributes[1].ShaderLocation = 1;
            vertexAttributes[1].Offset = 3 * sizeof(float);

            VertexBufferLayout vertexBufferLayout = new();
            vertexBufferLayout.StepMode = VertexStepMode.Vertex;
            vertexBufferLayout.Attributes = vertexAttributes;
            vertexBufferLayout.AttributeCount = 2;
            vertexBufferLayout.ArrayStride = 6 * sizeof(float);

            renderPipeline =
                WebGPUUtil.RenderPipeline.Create(engine, shaderModule, &vertexBufferLayout, pipelineLayout,
                primitiveTopology: PrimitiveTopology.LineList,
                label: Label);

            // Bind groups for resources.
            CreateBindGroups();

            // Dispose of shader module.
            engine.WGPU.ShaderModuleRelease(shaderModule);
        }

        public void Render(VertexBuffer vertexBuffer, uint vertexCount)
        {
            engine.WGPU.RenderPassEncoderSetPipeline(engine.CurrentRenderPassEncoder, renderPipeline);

            engine.WGPU.RenderPassEncoderSetBindGroup(engine.CurrentRenderPassEncoder,
                0,
                cameraBindGroup,
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
                vertexCount,
                1, 0, 0);
        }

        public void Dispose()
        {
            engine.WGPU.RenderPipelineRelease(renderPipeline);

            // Release layouts
            engine.WGPU.BindGroupLayoutRelease(cameraBindGroupLayout);

            // Release bind groups
            engine.WGPU.BindGroupRelease(cameraBindGroup);
        }
    }
}
