using FPS_Game.Utils;

namespace FPSGame.Utils
{
    public static class WebGPUUtil
    {
        public static BufferUtil Buffer { get; } = new BufferUtil();

        public static ShaderModuleUtil ShaderModule { get; } = new ShaderModuleUtil();

        public static RenderPipelineUtil RenderPipeline { get; } = new RenderPipelineUtil();
        
        public static TextureUtil Texture { get; } = new ();
        
        public static TextureViewUtil TextureView { get; } = new ();
        
        public static SamplerUtil Sampler { get; } = new ();
    }
}
