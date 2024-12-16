using FPSGame.Buffers;
using Silk.NET.Maths;

namespace FPSGame.Camera;

public interface ICamera
{
    UniformBuffer<Matrix4X4<float>> Buffer { get; }

    /// <summary>
    /// Buffer for the skybox projection view matrix. It does not contain the translation.
    /// </summary>
    UniformBuffer<Matrix4X4<float>> SkyboxProjectionViewBuffer { get; } 

    void Update();
}