using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPSGame.Utils
{
    internal static class WebGPUUtil
    {
        internal static BufferUtil BufferUtil { get; } = new BufferUtil();

        internal static ShaderModuleUtil ShaderModule { get; } = new ShaderModuleUtil();

        internal static RenderPipelineUtil RenderPipeline { get; } = new RenderPipelineUtil();
    }
}
