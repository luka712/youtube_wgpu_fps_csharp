﻿namespace FPSGame
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
    }
}
