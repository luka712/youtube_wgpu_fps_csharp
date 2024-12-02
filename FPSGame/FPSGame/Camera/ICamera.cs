using FPSGame.Buffers;
using Silk.NET.Maths;

namespace FPSGame.Camera;

public interface ICamera
{
    UniformBuffer<Matrix4X4<float>> Buffer { get; }
}