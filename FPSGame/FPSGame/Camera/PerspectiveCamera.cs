using FPSGame.Buffers;
using FPSGame.MathUtils;
using Silk.NET.Maths;

namespace FPSGame.Camera;

public class PerspectiveCamera : ICamera
{
    public Vector3D<float> Position { get; set; } = new(3, 3, -3);
    
    public Vector3D<float> Target { get; set; } = new(0, 0, 0);
    
    public Vector3D<float> Up { get; set; } = new(0, 1, 0);
    
    public float AspectRatio { get; set; } = 1.0f;
    
    public float FieldOfView { get; set; } = 60.0f;
    
    public float Near { get; set; } = 0.1f;
    
    public float Far { get; set; } = 100.0f;
    
    public UniformBuffer<Matrix4X4<float>> Buffer { get; private set; } = null!;

    public PerspectiveCamera(Engine engine)
    {
        Buffer = new UniformBuffer<Matrix4X4<float>>(engine, "Perspective Camera Buffer");
        Buffer.Initialize(Matrix4X4<float>.Identity);
    }

    public void Update()
    {
        Matrix4X4<float> perspective = Matrix4X4.CreatePerspectiveFieldOfView(
            MathUtils.MathUtils.DegToRad(FieldOfView),
            AspectRatio,
            Near,
            Far
        );
        
        Matrix4X4<float> view = Matrix4X4.CreateLookAt(Position, Target, Up);
        
        Buffer.Update(view * perspective);
    }
    
}