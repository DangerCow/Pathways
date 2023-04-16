using System.Numerics;
using ComputeSharp;

namespace Pathways.Shaders;

public class ShaderMath
{
    [ShaderMethod]
    public static Vector3 Transform(Vector3 value, Vector4 rotation)
    {
        float x2 = rotation.X + rotation.X;
        float y2 = rotation.Y + rotation.Y;
        float z2 = rotation.Z + rotation.Z;

        float wx2 = rotation.W * x2;
        float wy2 = rotation.W * y2;
        float wz2 = rotation.W * z2;
        float xx2 = rotation.X * x2;
        float xy2 = rotation.X * y2;
        float xz2 = rotation.X * z2;
        float yy2 = rotation.Y * y2;
        float yz2 = rotation.Y * z2;
        float zz2 = rotation.Z * z2;

        return new Vector3(
            value.X * (1.0f - yy2 - zz2) + value.Y * (xy2 - wz2) + value.Z * (xz2 + wy2),
            value.X * (xy2 + wz2) + value.Y * (1.0f - xx2 - zz2) + value.Z * (yz2 - wx2),
            value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1.0f - xx2 - yy2));
    }

    [ShaderMethod]
    public static Vector3 Transform(Vector3 value, float4x4 rotation)
    {
        return new Vector3(
            value.X * rotation.M11 + value.Y * rotation.M21 + value.Z * rotation.M31,
            value.X * rotation.M12 + value.Y * rotation.M22 + value.Z * rotation.M32,
            value.X * rotation.M13 + value.Y * rotation.M23 + value.Z * rotation.M33);
    }

    [ShaderMethod]
    public static float4x4 CreateMatrixFromQuaternion(Vector4 rotation)
    {
        float x2 = rotation.X + rotation.X;
        float y2 = rotation.Y + rotation.Y;
        float z2 = rotation.Z + rotation.Z;

        float wx2 = rotation.W * x2;
        float wy2 = rotation.W * y2;
        float wz2 = rotation.W * z2;
        float xx2 = rotation.X * x2;
        float xy2 = rotation.X * y2;
        float xz2 = rotation.X * z2;
        float yy2 = rotation.Y * y2;
        float yz2 = rotation.Y * z2;
        float zz2 = rotation.Z * z2;

        return new Float4x4(
            1.0f - yy2 - zz2, xy2 - wz2, xz2 + wy2, 0.0f,
            xy2 + wz2, 1.0f - xx2 - zz2, yz2 - wx2, 0.0f,
            xz2 - wy2, yz2 + wx2, 1.0f - xx2 - yy2, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f);
    }

    [ShaderMethod]
    public static Vector3 Normalize(Vector3 value)
    {
        float length = MathF.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
        return new Vector3(value.X / length, value.Y / length, value.Z / length);
    }
    
    [ShaderMethod]
    public static float Length(Vector3 value)
    {
        return MathF.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
    }
    
    [ShaderMethod]
    public static Vector3 Clamp(Vector3 x, Vector3 min, Vector3 max)
    {
        return new Vector3(Math.Clamp(x.X, min.X, max.X), Math.Clamp(x.Y, min.Y, max.Y), Math.Clamp(x.Z, min.Z, max.Z));
    }
    
    [ShaderMethod]
    public static Vector3 Smoothstep(Vector3 edge0, Vector3 edge1, Vector3 x)
    {
        Vector3 t = Clamp((x - edge0) / (edge1 - edge0), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
        return t * t * (new Vector3(3, 3, 3) - new Vector3(2, 2, 2) * t);
    }
    
    [ShaderMethod]
    public static Vector3 MulByScalar(Vector3 v, float s)
    {
        return new Vector3(v.X * s, v.Y * s, v.Z * s);
    }
    
    [ShaderMethod]
    public static Vector3 Abs(Vector3 v)
    {
        return new Vector3(MathF.Abs(v.X), MathF.Abs(v.Y), MathF.Abs(v.Z));
    }
    
    [ShaderMethod]
    public static Vector3 Min(Vector3 a, Vector3 b)
    {
        return new Vector3(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y), MathF.Min(a.Z, b.Z));
    }
    
    [ShaderMethod]
    public static Vector3 Max(Vector3 a, Vector3 b)
    {
        return new Vector3(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y), MathF.Max(a.Z, b.Z));
    }
}