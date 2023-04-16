using System.Numerics;
using ComputeSharp;

namespace Pathways.Shaders;

[AutoConstructor]
public readonly partial struct RayMarchShader : IComputeShader
{
    public const int MaxSteps = 256;

    public readonly ReadWriteBuffer<int> Buffer;
    public readonly int Width;
    public readonly int Height;

    public readonly ReadWriteBuffer<PathwayObject.ShaderRepresentation> Objects;

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
        rayDir = ShaderMath.Transform(rayDir, Camera.Rotation);
        rayDir = ShaderMath.Normalize(rayDir);


        Vector3 outColor = RayMarch(rayOrigin, rayDir);

        int outR = (int)Math.Clamp(outColor.X * 255, 0, 255);
        int outG = (int)Math.Clamp(outColor.Y * 255, 0, 255);
        int outB = (int)Math.Clamp(outColor.Z * 255, 0, 255);
        int outA = 255;
        int outInt = outA << 24 | outB << 16 | outG << 8 | outR;

        Buffer[ThreadIds.X] = outInt;
    }

    private Vector3 RayMarch(Vector3 rayOrigin, Vector3 rayDirection)
    {
        Vector3 hitColor = new Vector3();

        float t = 0;
        float tMax = 1000;

        for (int i = 0; i < MaxSteps; i++)
        {
            Vector3 point = rayOrigin + rayDirection * t;
            PathwayObject.ShaderRepresentation closestObject = new PathwayObject.ShaderRepresentation();
            closestObject.ObjectType = -1;

            float distance = float.MaxValue;
            
            for (int j = 0; j < Objects.Length; j++)
            {
                float objDistance = ObjectSdf(point, Objects[j]);
                distance = MathF.Min(distance, objDistance);
                
                if (objDistance == distance)
                {
                    closestObject = Objects[j];
                }
            }
            
            // dont allow negative distances
            if (distance < 0)
            {
                distance = float.MaxValue;
            }

            if (distance < 0.001f)
            {
                if (closestObject.ObjectType == -1) break;
                hitColor = closestObject.Color;
                break;
            }

            t += distance;
        }

        return hitColor;
    }

    // SDF FUNCTIONS

    private float ObjectSdf(Vector3 point, PathwayObject.ShaderRepresentation obj)
    {
        float distance = Sdf(point, obj.Position, obj.Scale, obj.Rotation, obj.ObjectType);

        return distance;
    }

    private float Sdf(Vector3 point, Vector3 center, Vector3 size, Vector4 rotation, int sdfType)
    {
        switch (sdfType)
        {
            // Sphere
            case 0:
                return SphereSdf(point, center, size.X);
            // Box
            case 1:
                return BoxSdf(point, center, size, rotation);
            // Plane
            case 2:
                return PlaneSdf(point, center, size, rotation);
            default:
                return 0;
        }
    }

    private float SphereSdf(Vector3 point, Vector3 center, float radius)
    {
        return ShaderMath.Length(point - center) - radius;
    }

    private float BoxSdf(Vector3 point, Vector3 center, Vector3 size, Vector4 rotation)
    {
        // rotate the point
        point = ShaderMath.Transform(point, rotation);
        center = ShaderMath.Transform(center, rotation);

        // translate the point
        point -= center;

        // get the distance from the point to the box
        Vector3 q = ShaderMath.Abs(point) - size;
        float d = ShaderMath.Length(ShaderMath.Max(q, new Vector3(0, 0, 0))) +
                  MathF.Min(MathF.Max(q.X, MathF.Max(q.Y, q.Z)), 0);

        // smooth the edges
        float k = 0.01f;
        return d - k;
    }

    private float PlaneSdf(Vector3 point, Vector3 center, Vector3 size, Vector4 rotation)
    {
        // not really a plane more like a quad but its fine ¯\_(ツ)_/¯
        size = new Vector3(size.X, 0, size.Z);

        // rotate the point
        point = ShaderMath.Transform(point, rotation);
        center = ShaderMath.Transform(center, rotation);

        // translate the point
        point -= center;

        //fix the world being surrounded by this quad
        if (!(point.X < -size.X || point.X > size.X || point.Y < -size.Y || point.Y > size.Y || point.Z < -size.Z ||
              point.Z > size.Z))
        {
            return float.MaxValue;
        }

        Vector3 d = ShaderMath.Abs(point - center) - size;
        return MathF.Min(MathF.Max(d.X, MathF.Max(d.Y, d.Z)), 0.0f) + ShaderMath.Length(Vector3.Max(d, Vector3.Zero));
    }

    private Vector3 GetNormal(Vector3 point, Vector3 center, Vector3 size, Vector4 rotation, int sdfType)
    {
        float h = 0.001f;
        Vector2 k = new Vector2(1, -1);
        Vector3 normal = new Vector3();

        Vector3 kxyy = new Vector3(k.X, k.Y, k.Y);
        Vector3 kyyx = new Vector3(k.Y, k.Y, k.X);
        Vector3 kyxy = new Vector3(k.Y, k.X, k.Y);
        Vector3 kxxx = new Vector3(k.X, k.X, k.X);

        normal += kxyy * Sdf(point + kxyy * h, center, size, rotation, sdfType);
        normal += kyyx * Sdf(point + kyyx * h, center, size, rotation, sdfType);
        normal += kyxy * Sdf(point + kyxy * h, center, size, rotation, sdfType);
        normal += kxxx * Sdf(point + kxxx * h, center, size, rotation, sdfType);

        // fix box normals


        return ShaderMath.Normalize(normal);
    }
}