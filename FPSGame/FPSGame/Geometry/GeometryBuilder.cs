using Silk.NET.WebGPU;

namespace FPSGame
{
    public class GeometryBuilder
    {
        public static Geometry CreateQuadGeometry()
        {
            return new()
            {
                Indices =
                [
                    0,1,2, // t0
                    1,3,2  // t1
                ],
                InterleavedVertices =
                [
                   -0.5f, -0.5f, 0f,    1, 1, 1, 1,   0,1, // v0
                    0.5f, -0.5f, 0f,    1, 1, 1, 1,   1,1, // v1
                   -0.5f,  0.5f, 0f,    1, 1, 1, 1,   0,0, // v2
                    0.5f,  0.5f, 0f,    1, 1, 1, 1,   1,0  // v3
                ],
                VertexCount = 6
            };
        }

        public static Geometry CreateCubeGeometry()
        {
            return new()
            {
                Indices =
                [
                    0, 1, 2, 0, 2, 3, // front
                    4, 5, 6, 4, 6, 7, // back
                    8, 9, 10, 8, 10, 11, // top
                    12, 13, 14, 12, 14, 15, // bottom
                    16, 17, 18, 16, 18, 19, // right
                    22, 20, 21, 20, 22, 23 // left
                ],
                InterleavedVertices =
                [
                    // Front face
                    -0.5f, -0.5f, 0.5f,     1, 1, 1, 1,    0, 1,
                    0.5f, -0.5f, 0.5f,      1, 1, 1, 1,    1, 1,
                    0.5f, 0.5f, 0.5f,       1, 1, 1, 1,    1, 0,
                    -0.5f, 0.5f, 0.5f,      1, 1, 1, 1,    0, 0,

                    // Back face
                    -0.5f, -0.5f, -0.5f,    1, 1, 1, 1,    1, 1,
                    -0.5f, 0.5f, -0.5f,     1, 1, 1, 1,    1, 0,
                    0.5f, 0.5f, -0.5f,      1, 1, 1, 1,    0, 0,
                    0.5f, -0.5f, -0.5f,     1, 1, 1, 1,    0, 1,

                    // Top face
                    -0.5f, 0.5f, -0.5f,     1, 1, 1, 1,    0, 1,
                    -0.5f, 0.5f, 0.5f,      1, 1, 1, 1,    0, 0,
                    0.5f, 0.5f, 0.5f,       1, 1, 1, 1,    1, 0,
                    0.5f, 0.5f, -0.5f,      1, 1, 1, 1,    1, 1,

                    // Bottom face
                    -0.5f, -0.5f, -0.5f,    1, 1, 1, 1,    0, 1,
                    0.5f, -0.5f, -0.5f,     1, 1, 1, 1,    1, 1,
                    0.5f, -0.5f, 0.5f,      1, 1, 1, 1,    1, 0,
                    -0.5f, -0.5f, 0.5f,     1, 1, 1, 1,    0, 0,

                    // Right face
                    0.5f, -0.5f, -0.5f,     1, 1, 1, 1,    1, 1,
                    0.5f, 0.5f, -0.5f,      1, 1, 1, 1,    1, 0,
                    0.5f, 0.5f, 0.5f,       1, 1, 1, 1,    0, 0,
                    0.5f, -0.5f, 0.5f,      1, 1, 1, 1,    0, 1,

                    // Left face
                    -0.5f, -0.5f, -0.5f,    1, 1, 1, 1,    0, 1,
                    -0.5f, -0.5f, 0.5f,     1, 1, 1, 1,    1, 1,
                    -0.5f, 0.5f, 0.5f,      1, 1, 1, 1,    1, 0,
                    -0.5f, 0.5f, -0.5f,     1, 1, 1, 1,    0, 0
                ],
                VertexCount = 24
            };
        }

        /// <inheritdoc />
        public static Geometry CreateSkyboxGeometry()
        {
            return new Geometry()
            {
                InterleavedVertices = new float[]
                {
                -1.0f, -1.0f, -1.0f, // triangle 1 : begin
                -1.0f, -1.0f, 1.0f,
                -1.0f, 1.0f, 1.0f, // triangle 1 : end
                1.0f, 1.0f, -1.0f, // triangle 2 : begin
                -1.0f, -1.0f, -1.0f,
                -1.0f, 1.0f, -1.0f, // triangle 2 : end
                1.0f, -1.0f, 1.0f,
                -1.0f, -1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,
                1.0f, 1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f, -1.0f,
                -1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f, -1.0f,
                1.0f, -1.0f, 1.0f,
                -1.0f, -1.0f, 1.0f,
                -1.0f, -1.0f, -1.0f,
                -1.0f, 1.0f, 1.0f,
                -1.0f, -1.0f, 1.0f,
                1.0f, -1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f, -1.0f, -1.0f,
                1.0f, 1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f, -1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f, 1.0f, -1.0f,
                -1.0f, 1.0f, -1.0f,
                1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f, -1.0f,
                -1.0f, 1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f, 1.0f,
                1.0f, -1.0f, 1.0f
                },
                VertexCount = 36
            };
        }

        public static Geometry CreateTerrainGeometry(int width, int length, float heightScaleFactor)
        {
            // Start from negative, so that terrain is always centered around (0,0,0).
            float zOffset = -length / 2.0f;
            float xOffset = -width / 2.0f;

            int vertexCount = (width + 1) * (length + 1);
            float[] vertices = new float[vertexCount * 3];
            float[] colors = new float[vertexCount * 4];
            float[] texCoords = new float[vertexCount * 2];
            ushort[] indices = new ushort[width * length * 6];

            int vertexIndex = 0;
            int colorIndex = 0;
            int texCoordIndex = 0;
            int indicesIndex = 0;

            Random rand = new Random();
            for (int z = 0; z <= length; z++)
            {
                for (int x = 0; x <= width; x++)
                {
                    vertices[vertexIndex++] = x + xOffset;
                    vertices[vertexIndex++] = (float) rand.NextDouble() * heightScaleFactor;
                    vertices[vertexIndex++] = z + zOffset;

                    colors[colorIndex++] = 1;
                    colors[colorIndex++] = 1;
                    colors[colorIndex++] = 1;
                    colors[colorIndex++] = 1;

                    // UV's are [0,1] for the entire terrain.
                    texCoords[texCoordIndex++] = x / (float)width;
                    texCoords[texCoordIndex++] = z / (float)length;
                }
            }

            for (int z = 0; z < length; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int bottomLeft = (width + 1) * z + x;
                    int topLeft = bottomLeft + width + 1;
                    int bottomRight = bottomLeft + 1;
                    int topRight = topLeft + 1;

                    // T1 - Top left, top right, bottom left.
                    indices[indicesIndex++] = (ushort)topLeft;
                    indices[indicesIndex++] = (ushort)topRight;
                    indices[indicesIndex++] = (ushort)bottomLeft;

                    // T2 - Bottom left, top right, bottom right.
                    indices[indicesIndex++] = (ushort)bottomLeft;
                    indices[indicesIndex++] = (ushort)topRight;
                    indices[indicesIndex++] = (ushort)bottomRight;
                }
            }


            // Now to interleaved
            float[] interleaved = new float[vertexCount * 9];
            int interleavedIndex = 0;
            vertexIndex = 0;
            colorIndex = 0;
            texCoordIndex = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                interleaved[interleavedIndex++] = vertices[vertexIndex++];
                interleaved[interleavedIndex++] = vertices[vertexIndex++];
                interleaved[interleavedIndex++] = vertices[vertexIndex++];

                interleaved[interleavedIndex++] = colors[colorIndex++];
                interleaved[interleavedIndex++] = colors[colorIndex++];
                interleaved[interleavedIndex++] = colors[colorIndex++];
                interleaved[interleavedIndex++] = colors[colorIndex++];

                interleaved[interleavedIndex++] = texCoords[texCoordIndex++];
                interleaved[interleavedIndex++] = texCoords[texCoordIndex++];
            }

            float[] heightData = new float[vertices.Length / 3];
            int heightDataIndex = 0;
            for (int i = 1; i < vertices.Length; i += 3)
            {
                heightData[heightDataIndex++] = vertices[i];
            }

            return new Geometry()
            {
                InterleavedVertices = interleaved,
                VertexCount = (uint)vertexCount,
                Indices = indices,
                HeightData = heightData
            };
        }
    }
}
