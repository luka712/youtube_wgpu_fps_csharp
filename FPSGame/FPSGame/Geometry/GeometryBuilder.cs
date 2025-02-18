using FPSGame.Texture;
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

        private static List<List<float>> HeightMapBytesToFloats(byte[] bytes, int width, int height)
        {
            List<List<float>> result = new List<List<float>>();

            for (int y = 0; y < height; y++)
            {
                result.Add(new List<float>());
                for (int x = 0; x < width; x++)
                {
                    float r = bytes[(y * width + x) * 4 + 0] / 255.0f;
                    float g = bytes[(y * width + x) * 4 + 1] / 255.0f;
                    float b = bytes[(y * width + x) * 4 + 2] / 255.0f;
                    float a = bytes[(y * width + x) * 4 + 3] / 255.0f;

                    result[y].Add((r + g + b) / 3.0f - 0.5f); // [-0.5, 0.5]
                }
            }

            return result;
        }

        public static Geometry CreateTerrainGeometry(int width, int length, float heightScaleFactor, Texture2D heightMapTexture)
        {
            byte[] bytes = heightMapTexture.GetPixels();
            List<List<float>> heightMap = HeightMapBytesToFloats(bytes, (int)heightMapTexture.Width, (int)heightMapTexture.Height);
            float maxHeight = heightMap.SelectMany(x => x).Max();
            float minHeight = heightMap.SelectMany(x => x).Min();

            // Start from negative, so that terrain is always centered around (0,0,0);
            float zOffset = -length / 2.0f;
            float xOffset = -width / 2.0f;

            int vertexCount = (width + 1) * (length + 1);
            float[] vertices = new float[vertexCount * 3];
            float[] colors = new float[vertexCount * 4];
            float[] texCoords = new float[vertexCount * 2];
            ushort[] indices = new ushort[6 * width * length];

            int vertexIndex = 0;
            int colorIndex = 0;
            int texCoordIndex = 0;
            int indicesIndex = 0;

            // TEMP
            Random rand = new Random();

            for (int z = 0; z <= length; z++)
            {
                for (int x = 0; x <= width; x++)
                {
                    float xNormal = x / (float)width;
                    float zZormal = z / (float)length;

                    int heighMapX = (int)(xNormal * (heightMapTexture.Width - 1));
                    int heightMapZ = (int)(zZormal * (heightMapTexture.Height - 1));

                    vertices[vertexIndex++] = x + xOffset;
                    vertices[vertexIndex++] = heightMap[heightMapZ][heighMapX] * heightScaleFactor;
                    vertices[vertexIndex++] = z + zOffset;

                    colors[colorIndex++] = 1;
                    colors[colorIndex++] = 1;
                    colors[colorIndex++] = 1;
                    colors[colorIndex++] = 1;

                    // UV's are [0,1] for entire terrain.
                    texCoords[texCoordIndex++] = xNormal;
                    texCoords[texCoordIndex++] = zZormal;
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

                    // Triangle 1 - Top left, top right, bottom left
                    indices[indicesIndex++] = (ushort)topLeft;
                    indices[indicesIndex++] = (ushort)topRight;
                    indices[indicesIndex++] = (ushort)bottomLeft;

                    // Triangle 2 - Bottom left, top right, bottom right
                    indices[indicesIndex++] = (ushort)bottomLeft;
                    indices[indicesIndex++] = (ushort)topRight;
                    indices[indicesIndex++] = (ushort)bottomRight;
                }
            }

            float[] interlaved = new float[vertexCount * 9]; // 3 for position, 4 for color, 2 for tex coords
            int interlavedIndex = 0;
            vertexIndex = 0;
            colorIndex = 0;
            texCoordIndex = 0;

            for (int i = 0; i < vertexCount; i++)
            {
                // (xyz) position
                interlaved[interlavedIndex++] = vertices[vertexIndex++];
                interlaved[interlavedIndex++] = vertices[vertexIndex++];
                interlaved[interlavedIndex++] = vertices[vertexIndex++];

                // (rgba) color
                interlaved[interlavedIndex++] = colors[colorIndex++];
                interlaved[interlavedIndex++] = colors[colorIndex++];
                interlaved[interlavedIndex++] = colors[colorIndex++];
                interlaved[interlavedIndex++] = colors[colorIndex++];

                // (uv) tex coords
                interlaved[interlavedIndex++] = texCoords[texCoordIndex++];
                interlaved[interlavedIndex++] = texCoords[texCoordIndex++];
            }

            // We just need to store Y-pos here.
            float[] heightData = new float[vertices.Length / 3]; // We just need Y.
            int heightDataIndex = 0;
            for (int i = 1; i < vertices.Length; i += 3)
            {
                heightData[heightDataIndex++] = vertices[i];
            }

            return new Geometry()
            {
                InterleavedVertices = interlaved,
                VertexCount = (uint)vertexCount,
                Indices = indices,
                HeightData = heightData,
            };
        }
    }
}
