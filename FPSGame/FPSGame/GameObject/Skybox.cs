using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Pipelines;
using FPSGame.Texture;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPSGame.GameObject
{
    internal class Skybox(Engine engine) : IDisposable
    {
        SkyboxRenderPipeline skyboxRenderPipeline = null!;
        CubeTexture skyboxTexture;

        SKImage leftImage = SKImage.FromEncodedData("Assets/xneg.png");
        SKImage rightImage = SKImage.FromEncodedData("Assets/xpos.png");
        SKImage topImage = SKImage.FromEncodedData("Assets/ypos.png");
        SKImage bottomImage = SKImage.FromEncodedData("Assets/yneg.png");
        SKImage frontImage = SKImage.FromEncodedData("Assets/zneg.png");
        SKImage backImage = SKImage.FromEncodedData("Assets/zpos.png");


        public void Initialize(ICamera camera)
        {
            skyboxTexture = new(engine,
               rightImage,
               leftImage,
               topImage,
               bottomImage,
               backImage,
               frontImage
               );
            skyboxTexture.Initialize();
            skyboxRenderPipeline = new SkyboxRenderPipeline(engine, skyboxTexture, camera, "Skybox Render Pipeline");
            skyboxRenderPipeline.Initialize();
        }

        public void Render()
        {
            skyboxRenderPipeline.Render();
        }
        public void Dispose()
        {
            skyboxRenderPipeline.Dispose();
            skyboxTexture.Dispose();
        }

    }
}
