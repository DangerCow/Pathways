using System.Numerics;
using ComputeSharp;

namespace Pathways.Shaders;

public class ShaderRotationMethods
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
}