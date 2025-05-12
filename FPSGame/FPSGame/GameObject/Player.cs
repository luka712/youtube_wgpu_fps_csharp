

using BulletSharp;
using BulletSharp.Math;
using FPSGame.Camera;
using FPSGame.Input;
using Silk.NET.Maths;

namespace FPSGame.GameObject
{
    internal class Player(Engine engine)
    {
        private KinematicCharacterController controller;
        private CapsuleShape shape;
        private FPSCamera camera;
        private float walkSpeed = 0.1f;
        private float yaw = 0;
        private float pitch = 0;

        public Vector2D<float> Position { get; private set; }

        public void Initialize(FPSCamera camera, DiscreteDynamicsWorld world)
        {
            this.camera = camera;
            camera.IsPlayerController = true;
            shape = new CapsuleShape(0.5f, 1.75f);

            // Create a kinematic character controller
            PairCachingGhostObject ghostObject = new PairCachingGhostObject();
            Matrix transform = Matrix.Identity;
            transform.Origin = new Vector3(0, 15, 0);
            ghostObject.WorldTransform = transform;
            ghostObject.CollisionShape = shape;
            ghostObject.CollisionFlags = CollisionFlags.CharacterObject;
            controller = new KinematicCharacterController(ghostObject, shape, 0.35f);

            // Add collision
            world.AddCollisionObject(ghostObject,
                CollisionFilterGroups.CharacterFilter,
                CollisionFilterGroups.StaticFilter | CollisionFilterGroups.DefaultFilter);
            world.AddAction(controller);
        }

        public void Update()
        {
            controller.GhostObject.GetWorldTransform(out Matrix transform);
            Vector3D<float> position = new(transform.Origin.X, transform.Origin.Y, transform.Origin.Z);

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

            float radPitch = FPSGame.MathUtils.MathUtil.DegToRad(pitch);
            float radYaw = FPSGame.MathUtils.MathUtil.DegToRad(yaw);

            Vector3D<float> target = new(0, 0, 0);
            target.X = MathF.Cos(radYaw) * MathF.Cos(radPitch);
            target.Y = MathF.Sin(radPitch);
            target.Z = MathF.Sin(radYaw) * MathF.Cos(radPitch);

            target = position + FPSGame.MathUtils.MathUtil.Normalize(target);

            Vector3D<float> forward = FPSGame.MathUtils.MathUtil.Normalize(target - position);
            Vector3D<float> right = FPSGame.MathUtils.MathUtil.Normalize(
                FPSGame.MathUtils.MathUtil.Cross(forward, camera.Up));

            KeyboardState keyboardState = engine.Input.GetKeyboardState();
            Vector3D<float> moveDirection = new(0, 0, 0);
            if (keyboardState.IsKeyDown(Silk.NET.Input.Key.W))
            {
                moveDirection += forward;
            }
            else if (keyboardState.IsKeyDown(Silk.NET.Input.Key.S))
            {
                moveDirection -= forward;
            }

            if (keyboardState.IsKeyDown(Silk.NET.Input.Key.A))
            {
                moveDirection -= right;
            }
            else if (keyboardState.IsKeyDown(Silk.NET.Input.Key.D))
            {
                moveDirection += right;
            }

            if (moveDirection != Vector3D<float>.Zero)
            {
                moveDirection = FPSGame.MathUtils.MathUtil.Normalize(moveDirection);
                moveDirection *= walkSpeed;
                controller.SetWalkDirection(new Vector3(moveDirection.X, 0, moveDirection.Z));
            }
            else
            {
                controller.SetWalkDirection(Vector3.Zero);
            }

            if (keyboardState.IsKeyDown(Silk.NET.Input.Key.Space) && controller.CanJump)
            {
                controller.Jump();
            }

            camera.Position = position;
            camera.Target = target;

            Position = new Vector2D<float>(position.X, position.Z);

        }
    }
}
