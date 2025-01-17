using BulletSharp;
using BulletSharp.Math;
using FPSGame;
using FPSGame.Buffers;
using FPSGame.Camera;
using WebGPU_FPS_Game.Pipelines;

namespace WebGPU_FPS_Game.DebugObjects
{
    internal class BulletDebugDrawable(Engine engine, ICamera camera) : DebugDraw
    {
        const int VERTEX_ELEMENTS = 6; // (xyz + rgb)
        const int VERTEX_ELEMENTS_PER_LINE = VERTEX_ELEMENTS * 2;
        float[] data = new float[VERTEX_ELEMENTS_PER_LINE * 100_000];


        private WireframePipeline pipeline;
        private VertexBuffer vertexBuffer = new VertexBuffer(engine);

        private int countOfVertices = 0;
        private int dataIndex = 0;

        public void Initialize()
        {
            pipeline = new WireframePipeline(engine, camera, "Bullet Debug Drawable");
            pipeline.Initialize();

            vertexBuffer.Initialize(data, 0);
        }

        public override DebugDrawModes DebugMode { get; set; } = DebugDrawModes.DrawWireframe;

        public override void Draw3DText(ref Vector3 location, string textString)
        {
            throw new NotImplementedException();
        }

        public override void DrawLine(ref Vector3 from, ref Vector3 to, ref Vector3 color)
        {
            if (countOfVertices >= 100_000)
            {
                return;
            }

            countOfVertices += 2;

            // v0
            data[dataIndex++] = from.X;
            data[dataIndex++] = from.Y;
            data[dataIndex++] = from.Z;
            data[dataIndex++] = color.X;
            data[dataIndex++] = color.Y;
            data[dataIndex++] = color.Z;

            // v1
            data[dataIndex++] = to.X;
            data[dataIndex++] = to.Y;
            data[dataIndex++] = to.Z;
            data[dataIndex++] = color.X;
            data[dataIndex++] = color.Y;
            data[dataIndex++] = color.Z;
        }

        public override void ReportErrorWarning(string warningString)
        {
            throw new NotImplementedException();
        }

        public void Render()
        {
            vertexBuffer.Update(data, dataIndex * sizeof(float));
            pipeline.Render(vertexBuffer, (uint)countOfVertices);
            countOfVertices = 0;
            dataIndex = 0;
        }
    }
}
