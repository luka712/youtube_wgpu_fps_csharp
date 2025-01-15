using BulletSharp;
using BulletSharp.Math;
using FPSGame.Buffers;
using FPSGame.Camera;
using FPSGame.Pipelines;
using Silk.NET.WebGPU;


namespace FPSGame.Debug
{
    internal class BulletWireframe(Engine engine, ICamera camera) : DebugDraw
    {
        private DebugBulletPhysicsPipeline pipeline;
        private VertexBuffer vertexBuffer = new VertexBuffer(engine);

        private int dataIndex = 0;
        private int countOfVertices = 0;

        const int VERTEX_ELEMENTS = 7; // (xyz) + (rgba).
        const int VERTICES_PER_LINE = 2 * VERTEX_ELEMENTS;
        float[] data = new float[VERTICES_PER_LINE * 100_000];

        public void Initialize()
        {
            pipeline = new DebugBulletPhysicsPipeline(engine, camera, "Bullet Wireframe");
            pipeline.Initialize();

            vertexBuffer.Initialize(data, 0);
        }

        public override DebugDrawModes DebugMode { get; set; }

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
            data[dataIndex++] = 1;

            // v1
            data[dataIndex++] = to.X;
            data[dataIndex++] = to.Y;
            data[dataIndex++] = to.Z;

            data[dataIndex++] = color.X;
            data[dataIndex++] = color.Y;
            data[dataIndex++] = color.Z;
            data[dataIndex++] = 1;
        }

        public override void ReportErrorWarning(string warningString)
        {
            throw new NotImplementedException();
        }

        public void Render()
        {
            vertexBuffer.Update(data, dataIndex * sizeof(float));
            pipeline.Render(vertexBuffer, (uint) countOfVertices);
            countOfVertices = 0;
            dataIndex = 0;
        }

        protected override void Dispose(bool disposing)
        {
            pipeline?.Dispose();
            vertexBuffer.Dispose();
        }
    }
}
