using FPSGame.Buffers;
using Silk.NET.Maths;

namespace FPSGame.Camera;

public interface ICamera
{
    /// <summary>
    /// The projection view buffer.
    /// </summary>
    UniformBuffer<Matrix4X4<float>> Buffer { get; }

    UniformBuffer<Matrix4X4<float>> ProjectionBuffer { get; }

    UniformBuffer<Matrix4X4<float>> ViewBuffer { get; }

    /// <summary>
    /// Buffer for the skybox projection view matrix. It does not contain the translation.
    /// </summary>
    UniformBuffer<Matrix4X4<float>> SkyboxProjectionViewBuffer { get; } 



    void Update();
}