using BulletSharp.Math;
using Silk.NET.Maths;

namespace FPSGame.Extensions
{
    internal static class Matrix4Extensions
    {
        public static Matrix ToBulletMatrix(this Matrix4X4<float> silkMatrix)
        {
            Matrix bulletMatrix = new Matrix();
            bulletMatrix.M11 = silkMatrix.M11;
            bulletMatrix.M12 = silkMatrix.M12;
            bulletMatrix.M13 = silkMatrix.M13;
            bulletMatrix.M14 = silkMatrix.M14;
            bulletMatrix.M21 = silkMatrix.M21;
            bulletMatrix.M22 = silkMatrix.M22;
            bulletMatrix.M23 = silkMatrix.M23;
            bulletMatrix.M24 = silkMatrix.M24;
            bulletMatrix.M31 = silkMatrix.M31;
            bulletMatrix.M32 = silkMatrix.M32;
            bulletMatrix.M33 = silkMatrix.M33;
            bulletMatrix.M34 = silkMatrix.M34;
            bulletMatrix.M41 = silkMatrix.M41;
            bulletMatrix.M42 = silkMatrix.M42;
            bulletMatrix.M43 = silkMatrix.M43;
            bulletMatrix.M44 = silkMatrix.M44;
            return bulletMatrix;
        }

        public static Matrix4X4<float> ToSilkNetMatrix(this Matrix bulletMatrix)
        {
            return new Matrix4X4<float>()
            {
                M11 = bulletMatrix.M11,
                M12 = bulletMatrix.M12,
                M13 = bulletMatrix.M13,
                M14 = bulletMatrix.M14,
                M21 = bulletMatrix.M21,
                M22 = bulletMatrix.M22,
                M23 = bulletMatrix.M23,
                M24 = bulletMatrix.M24,
                M31 = bulletMatrix.M31,
                M32 = bulletMatrix.M32,
                M33 = bulletMatrix.M33,
                M34 = bulletMatrix.M34,
                M41 = bulletMatrix.M41,
                M42 = bulletMatrix.M42,
                M43 = bulletMatrix.M43,
                M44 = bulletMatrix.M44
            };
        }
    }
}
