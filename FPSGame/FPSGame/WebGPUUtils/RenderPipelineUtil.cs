﻿using FPSGame.Extensions;
using Silk.NET.WebGPU;
using System.Runtime.InteropServices;

namespace FPSGame.Utils
{
  public unsafe class RenderPipelineUtil
    {
      public RenderPipeline* Create(
            Engine engine, 
            ShaderModule* shaderModule,
            PipelineLayout* pipelineLayout = null,
            string vertexFnName = "main_vs",
            string fragmentFnNAme = "main_fs",
            string label = "")
        {
            VertexAttribute* vertexAttributes = stackalloc VertexAttribute[3];

            // Vertex position
            vertexAttributes[0].Format = VertexFormat.Float32x3;
            vertexAttributes[0].ShaderLocation = 0;
            vertexAttributes[0].Offset = 0;

            // Vertex color
            vertexAttributes[1].Format = VertexFormat.Float32x4;
            vertexAttributes[1].ShaderLocation = 1;
            vertexAttributes[1].Offset = sizeof(float) * 3; // 12(xyz)
            
            // Vertex texture coords
            vertexAttributes[2].Format = VertexFormat.Float32x2;
            vertexAttributes[2].ShaderLocation = 2;
            vertexAttributes[2].Offset = sizeof(float) * (3 + 4); // 12(xyz) + 16(rgba)

            VertexBufferLayout layout = new VertexBufferLayout();
            layout.StepMode = VertexStepMode.Vertex;
            layout.Attributes = vertexAttributes;
            layout.AttributeCount = 3;
            layout.ArrayStride = 9 * sizeof(float);

            VertexState vertexState = new VertexState();
            vertexState.Module = shaderModule;
            vertexState.EntryPoint = (byte*)Marshal.StringToHGlobalAnsi(vertexFnName);
            vertexState.Buffers = &layout;
            vertexState.BufferCount = 1;

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
            fragmentState.EntryPoint = (byte*)Marshal.StringToHGlobalAnsi(fragmentFnNAme);
            fragmentState.Targets = colorTargetState;
            fragmentState.TargetCount = 1;

            // - DEPTH STENCIL
            StencilFaceState stencilFaceState = new StencilFaceState();
            stencilFaceState.Compare = CompareFunction.Always;
            stencilFaceState.FailOp = StencilOperation.Keep;
            stencilFaceState.DepthFailOp = StencilOperation.Keep;
            stencilFaceState.PassOp = StencilOperation.IncrementClamp;

            DepthStencilState depthStencilState = new()
            {
                DepthBias = 0,
                DepthWriteEnabled = true,
                DepthCompare = CompareFunction.LessEqual,
                Format = TextureFormat.Depth24PlusStencil8,
                StencilFront = stencilFaceState,
                StencilBack = stencilFaceState,
            };

            RenderPipelineDescriptor descriptor = new RenderPipelineDescriptor();
            descriptor.Label = label.ToBytePtr();
            descriptor.Layout = pipelineLayout;
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
                CullMode = CullMode.None,
                FrontFace = FrontFace.Ccw,
                Topology = PrimitiveTopology.TriangleList
            };
            descriptor.DepthStencil = &depthStencilState;

            return engine.WGPU.DeviceCreateRenderPipeline(engine.Device, descriptor);
        }
    }
}
