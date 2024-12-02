using System.Numerics;
using FPSGame.Buffers;
using FPSGame.Input;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace FPSGame.Camera;

public class FPSCamera : ICamera
{
    private readonly Engine engine;

    private Vector3D<float> cameraUp = Vector3D<float>.UnitY;
    private Vector3D<float> cameraFront;
    private float yaw = 0;
    private float pitch;

    public FPSCamera(Engine engine)
    {
        this.engine = engine;
        Buffer = new UniformBuffer<Matrix4X4<float>>(engine, "FPS Camera Buffer");
        Buffer.Initialize(Matrix4X4<float>.Identity);
    }
    
    public UniformBuffer<Matrix4X4<float>> Buffer { get; private set; } = null!;
    
    public Vector3D<float> Position { get; set; } = new(0, 0, 0);

    public void Update()
    {
        Matrix4X4<float> perspective = Matrix4X4.CreatePerspectiveFieldOfView(
            MathUtils.MathUtils.DegToRad(60),
            engine.Window.Size.X / (float) engine.Window.Size.Y,
            0.1f,
            1000f
        );
        
        MouseState mouseState = engine.Input.GetMouseState();

        if(mouseState.DeltaX != 0 || mouseState.DeltaY != 0)
        {
            yaw += mouseState.DeltaX;
            pitch -= mouseState.DeltaY;
        }
        
        if(pitch > 89.0f)
        {
            pitch = 89.0f;
        }
        else if(pitch < -89.0f)
        {
            pitch = -89.0f;
        }
        
        float yawRad = MathUtils.MathUtils.DegToRad(yaw);
        float pitchRad = MathUtils.MathUtils.DegToRad(pitch);

        cameraFront.X = MathF.Cos(pitchRad) * MathF.Cos(yawRad);
        cameraFront.Y = MathF.Sin(pitchRad);
        cameraFront.Z = MathF.Cos(pitchRad) * MathF.Sin(yawRad);
        cameraFront = MathUtils.MathUtils.Normalize(cameraFront);
        cameraFront = Position + cameraFront;
        
        // Let's solve new position.
        KeyboardState keyboardState = engine.Input.GetKeyboardState();
        Vector3D<float> forwardVector = cameraFront - Position;
        
        forwardVector.Y = 0; // No up and down movement.
        forwardVector = MathUtils.MathUtils.Normalize(forwardVector);
        
        Vector3D<float> rightVector = MathUtils.MathUtils.Cross(forwardVector, cameraUp);
        rightVector = MathUtils.MathUtils.Normalize(rightVector);

        if (keyboardState.IsKeyDown(Key.W))
        {
            Position += forwardVector * 0.1f; 
        }
        else if (keyboardState.IsKeyDown(Key.S))
        {
            Position -= forwardVector * 0.1f;
        }
        
        if(keyboardState.IsKeyDown(Key.A))
        {
            Position -= rightVector * 0.1f;
            cameraFront -= rightVector * 0.1f;
        }
        else if(keyboardState.IsKeyDown(Key.D))
        {
            Position += rightVector * 0.1f;
            cameraFront += rightVector * 0.1f;
        }
        
        Matrix4X4<float> view = Matrix4X4.CreateLookAt(Position, cameraFront, Vector3D<float>.UnitY);
        
        Buffer.Update(view * perspective);
    }
   
}
