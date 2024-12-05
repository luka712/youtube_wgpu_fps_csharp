using Silk.NET.Maths;

namespace FPSGame.MathUtils;

public class MathUtil
{
    public static float DegToRad(float degrees)
    {
        return degrees * (float)Math.PI / 180.0f;
    }

    public static Vector3D<float> Normalize(Vector3D<float> v)
    {
        float l = v.Length;
        return new Vector3D<float>(v.X / l, v.Y / l, v.Z / l);
    }

    public static Vector3D<float> Cross(Vector3D<float> a, Vector3D<float> b)
    {
        return new Vector3D<float>(
             a.Y * b.Z - a.Z * b.Y,
             a.Z * b.X - a.X * b.Z,
             a.X * b.Y - a.Y * b.X
       );
    }
}