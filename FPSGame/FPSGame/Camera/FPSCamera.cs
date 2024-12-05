using System.Numerics;
using FPSGame.Buffers;
using FPSGame.Input;
using FPSGame.MathUtils;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace FPSGame.Camera;

public class FPSCamera : ICamera
{
    private readonly Engine engine;

    /// <summary>
    /// The rotation around the Y axis.
    /// </summary>
    private float yaw = 0.0f;

    /// <summary>
    /// The rotation around the X axis. 
    /// </summary>
    private float pitch = 0.0f;

    public Vector3D<float> Position { get; set; } = new(3, 3, -3);

    public Vector3D<float> Up { get; set; } = new(0, 1, 0);

    public float AspectRatio { get; set; } = 1.0f;

    public float FieldOfView { get; set; } = 60.0f;

    public float Near { get; set; } = 0.1f;

    public float Far { get; set; } = 100.0f;

    public UniformBuffer<Matrix4X4<float>> Buffer { get; private set; } = null!;

    public FPSCamera(Engine engine)
    {
        this.engine = engine;
        Buffer = new UniformBuffer<Matrix4X4<float>>(engine, "Perspective Camera Buffer");
        Buffer.Initialize(Matrix4X4<float>.Identity);
    }

    public void Update()
    {
        Matrix4X4<float> perspective = Matrix4X4.CreatePerspectiveFieldOfView(
            MathUtil.DegToRad(FieldOfView),
            AspectRatio,
            Near,
            Far
        );

        MouseState mouseState = engine.Input.GetMouseState();

        // TODO: we need deltatime here.
        pitch -= mouseState.DeltaY * 0.5f;
        yaw += mouseState.DeltaX * 0.5f;

        if (pitch > 89.0f)
        {
            pitch = 89.0f;
        }
        else if (pitch < -89.0f)
        {
            pitch = -89.0f;
        }

        float radPitch = MathUtil.DegToRad(pitch);
        float radYaw = MathUtil.DegToRad(yaw);

        Vector3D<float> target = new(0, 0, 0);
        target.X = MathF.Cos(radYaw) * MathF.Cos(radPitch);
        target.Y = MathF.Sin(radPitch);
        target.Z = MathF.Sin(radYaw) * MathF.Cos(radPitch);

        target = Position + MathUtil.Normalize(target);

        Vector3D<float> forward = MathUtil.Normalize(target - Position);
        Vector3D<float> right = MathUtil.Normalize(MathUtil.Cross(forward, Up));

        KeyboardState keyboardState = engine.Input.GetKeyboardState();
        if (keyboardState.IsKeyDown(Silk.NET.Input.Key.W))
        {
            Position += forward * 0.1f;
        }
        else if (keyboardState.IsKeyDown(Silk.NET.Input.Key.S))
        {
            Position -= forward * 0.1f;
        }

        if (keyboardState.IsKeyDown(Silk.NET.Input.Key.A))
        {
            Position -= right * 0.1f;
            target -= right * 0.1f;
        }
        else if (keyboardState.IsKeyDown(Silk.NET.Input.Key.D))
        {
            Position += right * 0.1f;
            target += right * 0.1f;
        }

        Matrix4X4<float> view = Matrix4X4.CreateLookAt(Position, target, Up);


        Buffer.Update(view * perspective);
    }

}
