using System.Numerics;
using ComputeSharp;
using TerraFX.Interop.Windows;

namespace Pathways.Shaders;

[AutoConstructor]
public readonly partial struct RaymarchShader : IComputeShader
{
    public readonly ReadWriteBuffer<Vector4> Output;

    public readonly int Width;
    public readonly int Height;

    public readonly ReadWriteBuffer<GameObject.ShaderRepresentation> Objects;
    public readonly Camera.ShaderRepresentation Camera;

    public void Execute()
    {
        int x = ThreadIds.X % Width;
        int y = ThreadIds.X / Width;

        float u = x / (float)Width;
        u = (u * 2) - 1;
        float v = y / (float)Height;
        v = (v * 2) - 1;

        float tanHalfFov = MathF.Tan((MathF.PI / 180) * Camera.Fov / 2);
        float aspectRatio = Width / (float)Height;

        u = u * tanHalfFov * aspectRatio;
        v = v * tanHalfFov;

        Vector3 rayOrigin = Camera.Position;

        Vector3 rayDir = new Vector3(u, -v, -1);
        rayDir = ShaderRotationMethods.Transform(rayDir, Camera.Rotation);
        rayDir = ShaderRotationMethods.Normalize(rayDir);

        Output[ThreadIds.X] = Raymarch(rayOrigin, rayDir);
    }

    private Vector4 Raymarch(Vector3 rayOrigin, Vector3 rayDir)
    {
        Vector4 hitColor = new Vector4(0, 0, 0, 1);

        float t = 0;
        float tMax = 100;
        int maxIterations = 128;

        for (int i = 0; i < maxIterations; i++)
        {
            Vector3 point = rayOrigin + rayDir * t;

            float minDistance = float.MaxValue;

            for (int j = 0; j < Objects.Length; j++)
            {
                GameObject.ShaderRepresentation obj = Objects[j];
                int sdfType = obj.SdfType;

                float distance = 0;

                switch (sdfType)
                {
                    case 0:
                        distance = SphereSdf(point, obj.Position, obj.Scale.X);
                        break;
                    case 1:
                        distance = BoxSdf(point, obj.Position, obj.Scale, obj.Rotation);
                        break;
                    case 2:
                        distance = PlaneSdf(point, obj.Position, obj.Scale, obj.Rotation);
                        break;
                }
                
                // dont allow negative distances
                if (distance < 0)
                {
                    distance = float.MaxValue;
                }

                if (distance < minDistance)
                {
                    Vector4 color;
                    color.X = obj.Color.X;
                    color.Y = obj.Color.Y;
                    color.Z = obj.Color.Z;
                    color.W = 1;

                    minDistance = distance;
                    hitColor = color;

                    if (distance < 0.001f)
                    {
                        return hitColor;
                    }
                }
            }
            
            t += minDistance;
        }

        return new Vector4(t, t, t, 1);
    }
    
    // SDF functions
    
    private float SphereSdf(Vector3 point, Vector3 center, float radius)
    {
        return Vector3.Distance(point, center) - radius;
    }
    
    private float BoxSdf(Vector3 point, Vector3 center, Vector3 size, Vector4 rotation)
    {
        // rotate the point
        point = ShaderRotationMethods.Transform(point, rotation);
        center = ShaderRotationMethods.Transform(center, rotation);
        
        // translate the point
        point -= center;
        
        //fix the world being surrounded by this box
        if (!(point.X < -size.X || point.X > size.X || point.Y < -size.Y || point.Y > size.Y || point.Z < -size.Z || point.Z > size.Z))
        {
            return float.MaxValue;
        }

        // get the distance from the point to the box
        Vector3 d = Abs(point) - size;
        return MathF.Min(MathF.Max(d.X, MathF.Max(d.Y, d.Z)), 0.0f) + Length(Vector3.Max(d, Vector3.Zero));
    }
    
    private float PlaneSdf(Vector3 point, Vector3 center, Vector3 size, Vector4 rotation)
    {
        // not really a plane more like a quad but its fine ¯\_(ツ)_/¯
        size = new Vector3(size.X, 0, size.Z);
        
        // rotate the point
        point = ShaderRotationMethods.Transform(point, rotation);
        center = ShaderRotationMethods.Transform(center, rotation);
        
        // translate the point
        point -= center;
        
        //fix the world being surrounded by this quad
        if (!(point.X < -size.X || point.X > size.X || point.Y < -size.Y || point.Y > size.Y || point.Z < -size.Z || point.Z > size.Z))
        {
            return float.MaxValue;
        }
        Vector3 d = Abs(point - center) - size;
        return MathF.Min(MathF.Max(d.X, MathF.Max(d.Y, d.Z)), 0.0f) + Length(Vector3.Max(d, Vector3.Zero));
    }
    
    // utility functions
    
    [ShaderMethod]
    private static Vector3 Abs(Vector3 v)
    {
        return new Vector3(MathF.Abs(v.X), MathF.Abs(v.Y), MathF.Abs(v.Z));
    }
    
    [ShaderMethod]
    private static Vector3 Min(Vector3 a, Vector3 b)
    {
        return new Vector3(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y), MathF.Min(a.Z, b.Z));
    }
    
    [ShaderMethod]
    private static Vector3 Max(Vector3 a, Vector3 b)
    {
        return new Vector3(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y), MathF.Max(a.Z, b.Z));
    }
    
    [ShaderMethod]
    private static float Length(Vector3 v)
    {
        return MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
    }
}