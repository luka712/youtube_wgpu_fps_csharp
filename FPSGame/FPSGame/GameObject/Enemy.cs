using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Texture;
using Silk.NET.Maths;
using SkiaSharp;
using WebGPU_FPS_Game.GameObjects;
using WebGPU_FPS_Game.Pipelines;

namespace FPSGame.GameObject
{
    public enum AnimationState
    {
        Stand,
        Walk,
        Die
    }

    internal class Enemy(Engine engine) : IDisposable
    {
        private Terrain terrain;

        const float ANIMATION_SPEED = 0.1f;
        const float FRAME_CHANGE_TIME = 1f;

        // 3 floats for position, 4 for rotation, 2 for scale * 4 for number of vertices for quad.
        private float[] data = new float[(3 + 4 + 2) * 4];

        float currentAnimationTime = 0;
        int totalFrames = 1;
        int currentAnimation = 0;

        BillboardRenderPipeline pipeline = null!;
        VertexBuffer vertexBuffer = new VertexBuffer(engine);
        IndexBuffer indexBuffer = new IndexBuffer(engine);

        SKImage standImg = SKImage.FromEncodedData("Assets/frogmon_stand.png");
        SKImage walkImg = SKImage.FromEncodedData("Assets/frogmon_walk.png");
        SKImage dieImg = SKImage.FromEncodedData("Assets/frogmon_die.png");

        Texture2D? standTex = null;
        Texture2D? walkTex = null;
        Texture2D? dieTex = null;

        Matrix4X4<float>[] transform = [Matrix4X4<float>.Identity];
        InstanceBuffer<Matrix4X4<float>> transformsBuffer = new(engine);

        /// <summary>
        /// The x and z position on the terrain.
        /// </summary>
        public Vector2D<float> Position { get; set; }

        private AnimationState animation;

        public AnimationState Animation
        {
            get => animation;
            set
            {
                animation = value;
                currentAnimation = 0;

                if (Animation == AnimationState.Stand)
                {
                    totalFrames = 1;
                    pipeline.Texture = standTex;
                }
                else if (Animation == AnimationState.Walk)
                {
                    totalFrames = 4;
                    pipeline.Texture = walkTex;
                }
                else if (Animation == AnimationState.Die)
                {
                    totalFrames = 5;
                    pipeline.Texture = dieTex;
                }
            }
        }

        private void SetupVertexData(int animationFrame, int totalFrames)
        {
            int i = 0;

            float texCoordStep = 1f / totalFrames;
            float u = animationFrame * texCoordStep;

            // Top left.
            WriteVertex(ref i, -0.5f, 0.5f, u, 0);
            // Top right
            WriteVertex(ref i, 0.5f, 0.5f, u + texCoordStep, 0);
            // Bottom right
            WriteVertex(ref i, 0.5f, -0.5f, u + texCoordStep, 1);
            // Bottom left
            WriteVertex(ref i, -0.5f, -0.5f, u, 1);
        }

        private void WriteVertex(ref int index, float x, float y, float u, float v)
        {
            data[index++] = x;
            data[index++] = y;
            data[index++] = 0;
            data[index++] = 1;
            data[index++] = 1;
            data[index++] = 1;
            data[index++] = 1;
            data[index++] = u;
            data[index++] = v;
        }

        public void Initialize(ICamera camera, Terrain terrain)
        {
            this.terrain = terrain;

            transformsBuffer.Initialize(transform);

            pipeline = new BillboardRenderPipeline(engine, transformsBuffer, camera, "Decal Render Pipeline");

            standTex = new Texture2D(engine, standImg, "Stand");
            standTex.Initialize();

            walkTex = new Texture2D(engine, walkImg, "Walk");
            walkTex.Initialize();

            dieTex = new Texture2D(engine, dieImg, "Die");
            dieTex.Initialize();

            pipeline.Initialize();
            pipeline.Texture = standTex;

            // VertexCount is not relevant, since we draw with indices.
            vertexBuffer.Initialize(data, 4);
            indexBuffer.Initialize([0, 1, 2, 2, 3, 0]);
        }

        public void Update()
        {
            // TRANSFORM
            float x = Position.X + 32;
            float z = Position.Y + 32;

            int terrainIndex = (int)z * (terrain.TerrainWidth + 1) + (int)x + 1;
            float height = terrain.HeightData[terrainIndex];

            x -= 32.0f;
            z -= 32.0f;

            transform[0] = Matrix4X4.CreateScale(2, 2, 1f) * Matrix4X4.CreateTranslation(
             x,
             height + 1f,
             z);

            transformsBuffer.Update(transform[0], 0);

            // ANIMATION
            if (currentAnimationTime > FRAME_CHANGE_TIME)
            {
                currentAnimationTime = 0;
                currentAnimation++;

                if (currentAnimation >= totalFrames)
                {
                    currentAnimation = 0;
                }

                SetupVertexData(currentAnimation, totalFrames);
                vertexBuffer.Update(data);
            }
            currentAnimationTime += ANIMATION_SPEED;
        }

        public void Render()
        {
            pipeline.Render(vertexBuffer, indexBuffer);
        }

        public void Dispose()
        {
            pipeline?.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            standTex?.Dispose();
            walkTex?.Dispose();
            dieTex?.Dispose();
        }
    }
}
