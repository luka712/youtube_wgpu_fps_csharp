using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

namespace FPSGame.Pipelines;

public unsafe class UnlitRenderPipeline
{
    private readonly Engine engine;
    private RenderPipeline* renderPipeline;

    public UnlitRenderPipeline(Engine engine)
    {
        this.engine = engine;
    }

    private ShaderModule* CreateShaderModule()
    {
        string shaderCode = File.ReadAllText("Shaders/unlit.wgsl");
        
        ShaderModuleWGSLDescriptor wgslDescriptor = new ShaderModuleWGSLDescriptor();
        wgslDescriptor.Code = (byte*)Marshal.StringToHGlobalAnsi(shaderCode);
        wgslDescriptor.Chain.SType = SType.ShaderModuleWgslDescriptor;

        ShaderModuleDescriptor descriptor = new ShaderModuleDescriptor();
        descriptor.NextInChain = (ChainedStruct*)&wgslDescriptor;
        
        ShaderModule* shaderModule = engine.WGPU.DeviceCreateShaderModule(engine.Device, descriptor);
        
        Console.WriteLine("Shader Module Created");

        return shaderModule;
    }
    
    public void Initialize()
    {
        ShaderModule* shaderModule = CreateShaderModule();

        VertexState vertexState = new VertexState();
        vertexState.Module = shaderModule;
        vertexState.EntryPoint = (byte*)Marshal.StringToHGlobalAnsi("main_vs");

        BlendState* blendState = stackalloc BlendState[1];
        blendState[0].Color = new BlendComponent()
        {
            SrcFactor = BlendFactor.One,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Operation = BlendOperation.Add
        };
        blendState[0].Alpha = new BlendComponent()
        {
            SrcFactor = BlendFactor.One,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
            Operation = BlendOperation.Add
        };

        ColorTargetState* colorTargetState = stackalloc ColorTargetState[1];
        colorTargetState[0].WriteMask = ColorWriteMask.All;
        colorTargetState[0].Format = engine.PreferredTextureFormat;
        colorTargetState[0].Blend = blendState;

        FragmentState fragmentState = new FragmentState();
        fragmentState.Module = shaderModule;
        fragmentState.EntryPoint = (byte*)Marshal.StringToHGlobalAnsi("main_fs");
        fragmentState.Targets = colorTargetState;
        fragmentState.TargetCount = 1;


        RenderPipelineDescriptor descriptor = new RenderPipelineDescriptor();
        descriptor.Vertex = vertexState;
        descriptor.Fragment = &fragmentState;
        descriptor.Multisample = new MultisampleState()
        {
            Mask = 0xFFFFFFF,
            Count = 1,
            AlphaToCoverageEnabled = false
        };
        descriptor.Primitive = new PrimitiveState()
        {
            CullMode = CullMode.Back,
            FrontFace = FrontFace.Ccw,
            Topology =  PrimitiveTopology.TriangleList
        };

        renderPipeline = engine.WGPU.DeviceCreateRenderPipeline(engine.Device, descriptor);
        Console.WriteLine("Pipeline Created");
    }

    public void Render()
    {
        engine.WGPU.RenderPassEncoderSetPipeline(engine.CurrentRenderPassEncoder, renderPipeline);
        engine.WGPU.RenderPassEncoderDraw(engine.CurrentRenderPassEncoder, 3, 1, 0, 0);
    }
}