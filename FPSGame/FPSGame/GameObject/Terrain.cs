using BulletSharp;
using FPSGame.Buffers;
using FPSGame.Pipelines;
using FPSGame.Texture;
using SkiaSharp;

using FPSGame;
using FPSGame.Camera;

namespace WebGPU_FPS_Game.GameObjects
{
    internal unsafe class Terrain(Engine engine, DiscreteDynamicsWorld world)
    {
        TerrainRenderPipeline pipeline = null!;
        VertexBuffer vertexBuffer = new VertexBuffer(engine);
        IndexBuffer indexBuffer = new IndexBuffer(engine);
        SKImage heightMapImage = SKImage.FromEncodedData("Assets/heightmap.png");
        SKImage splatMapImage = SKImage.FromEncodedData("Assets/splatmap.png");
        SKImage grassImage = SKImage.FromEncodedData("Assets/grass.png");
        SKImage rockImage = SKImage.FromEncodedData("Assets/rock.png");
        SKImage dirtImage = SKImage.FromEncodedData("Assets/dirt.png");
        Texture2D? heightMapTexture = null;
        Texture2D? splatMapTexture = null;
        Texture2D? grassTexture = null;
        Texture2D? rockTexture = null;
        Texture2D? dirtTexture = null;
        RigidBody rigidBody = null!;
        
        
        public float[] HeightData { get; private set; }

        public int TerrainWidth { get; private set; } = 64;

        public int TerrainLength { get; private set; } = 64;

        public float TerrainHeightFactor { get; private set; }

        public void Initialize(ICamera camera)
        {
            pipeline = new TerrainRenderPipeline(engine, camera, "Unlit Render Pipeline");

            heightMapTexture = new Texture2D(engine, heightMapImage, "Texture2D", true);
            heightMapTexture.Initialize();

            splatMapTexture = new Texture2D(engine, splatMapImage, "Texture2D Splat Map", true);
            splatMapTexture.Initialize();

            grassTexture = new Texture2D(engine, grassImage, "Texture2D Grass", true);
            grassTexture.Initialize();

            rockTexture = new Texture2D(engine, rockImage, "Texture2D Rock", true);
            rockTexture.Initialize();

            dirtTexture = new Texture2D(engine, dirtImage, "Texture2D Dirt", true);
            dirtTexture.Initialize();

            pipeline.Initialize();
            pipeline.MixTexture = splatMapTexture;
            pipeline.RedTexture = dirtTexture;
            pipeline.GreenTexture = grassTexture;
            pipeline.BlueTexture = rockTexture;
            pipeline.TextureTilling = new(16, 16);

            TerrainHeightFactor = 11;
            Geometry terrainGeometry = GeometryBuilder.CreateTerrainGeometry(
                TerrainWidth, TerrainLength, TerrainHeightFactor, heightMapTexture);

            // VertexCount is not relevant, since we draw with indices.
            vertexBuffer.Initialize(terrainGeometry.InterleavedVertices, terrainGeometry.VertexCount);
            indexBuffer.Initialize(terrainGeometry.Indices);

            Random rand = new Random();

            // PHYSICS
            HeightData = terrainGeometry.HeightData;
            fixed (float* heightDataPtr = HeightData)
            {
                float minHeight = TerrainHeightFactor * -.5f;
                float maxHeight = TerrainHeightFactor * .5f;

                CollisionShape shape = new HeightfieldTerrainShape(
                    TerrainWidth + 1,
                    TerrainLength + 1,
                    (IntPtr)heightDataPtr,
                    TerrainHeightFactor,
                    minHeight, maxHeight,
                    1,
                    PhyScalarType.Single,
                    false);
                MotionState motionState = new DefaultMotionState();
                RigidBodyConstructionInfo constructionInfo = new RigidBodyConstructionInfo(0, motionState, shape);
                rigidBody = new RigidBody(constructionInfo);
                world.AddRigidBody(rigidBody);
            }
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
            heightMapTexture?.Dispose();
        }
    }
}
