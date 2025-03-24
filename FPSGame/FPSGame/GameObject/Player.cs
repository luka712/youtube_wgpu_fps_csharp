

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

        private float WalkSpeed = 0.1f;

        /// <summary>
        /// The rotation around the Y axis.
        /// </summary>
        private float yaw = 0.0f;

        /// <summary>
        /// The rotation around the X axis. 
        /// </summary>
        private float pitch = 0.0f;

        public void Initialize(DiscreteDynamicsWorld world, BroadphaseInterface broadphase, FPSCamera camera)
        {
            this.camera = camera;

            shape = new CapsuleShape(0.5f, 1.75f);
            shape.Margin = 0.05f;
            Matrix transform = Matrix.Identity;
            transform.Origin = new Vector3(0, 15, 0);

            // Setup ghost object for collision detection
            PairCachingGhostObject ghostObject = new PairCachingGhostObject();
            broadphase.OverlappingPairCache.SetInternalGhostPairCallback(new GhostPairCallback());
            ghostObject.WorldTransform = transform;
            ghostObject.CollisionShape = shape;
            ghostObject.CollisionFlags = CollisionFlags.CharacterObject;

            // Setup the KinematicCharacterController
            controller = new KinematicCharacterController(ghostObject, shape, 0.35f);

            // Add collision.
            world.AddCollisionObject(ghostObject,
                CollisionFilterGroups.CharacterFilter,
                CollisionFilterGroups.StaticFilter | CollisionFilterGroups.DefaultFilter);
            world.AddAction(controller);
        }

        public void Update()
        {
            controller.GhostObject.GetWorldTransform(out Matrix transform);
            Vector3D<float> position = new Vector3D<float>(transform.Origin.X, transform.Origin.Y, transform.Origin.Z);

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

            float radPitch = MathUtils.MathUtil.DegToRad(pitch);
            float radYaw = MathUtils.MathUtil.DegToRad(yaw);

            Vector3D<float> target;
            target.X = MathF.Cos(radYaw) * MathF.Cos(radPitch);
            target.Y = MathF.Sin(radPitch);
            target.Z = MathF.Sin(radYaw) * MathF.Cos(radPitch);

            target = position + MathUtils.MathUtil.Normalize(target);

            Vector3D<float> forward = MathUtils.MathUtil.Normalize(target - position);
            Vector3D<float> right = MathUtils.MathUtil.Normalize(MathUtils.MathUtil.Cross(forward, camera.Up));

            KeyboardState keyboardState = engine.Input.GetKeyboardState();
            Vector3D<float> direction = new Vector3D<float>(0, 0, 0);
            if (keyboardState.IsKeyDown(Silk.NET.Input.Key.W))
            {
                direction += forward;
            }
            else if (keyboardState.IsKeyDown(Silk.NET.Input.Key.S))
            {
                direction -= forward;
            }

            if (keyboardState.IsKeyDown(Silk.NET.Input.Key.A))
            {
                direction -= right;
            }
            else if (keyboardState.IsKeyDown(Silk.NET.Input.Key.D))
            {
                direction += right;
            }

            if (direction != Vector3D<float>.Zero)
            {
                direction = MathUtils.MathUtil.Normalize(direction);
                direction *= WalkSpeed;
                controller.SetWalkDirection(new Vector3(direction.X, 0, direction.Z));
            }
            else
            {
                controller.SetWalkDirection(Vector3.Zero);
            }

            if(keyboardState.IsKeyReleased(Silk.NET.Input.Key.Space) && controller.CanJump && controller.OnGround)
            {
                controller.Jump();
            }

            camera.Position = position;
            camera.Target = target;
        }
    }
}
