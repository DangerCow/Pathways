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
    public readonly ReadWriteBuffer<LightSource.ShaderRepresentation> Lights;
    public readonly Camera.ShaderRepresentation Camera;
    
    private const int MaxIterations = 512;

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

        for (int i = 0; i < MaxIterations; i++)
        {
            Vector3 point = rayOrigin + rayDir * t;

            float minDistance = float.MaxValue;

            for (int j = 0; j < Objects.Length; j++)
            {
                GameObject.ShaderRepresentation obj = Objects[j];
                int sdfType = obj.SdfType;

                float distance = GetDist(point, obj.Position, obj.Scale, obj.Rotation, sdfType);
                
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
                        // we hit something
                        Vector3 normal = GetNormal(point, obj.Position, obj.Scale, obj.Rotation, sdfType);
                        return Shade(point, normal, hitColor);
                    }
                }
            }
            
            t += minDistance;
        }

        return new Vector4(t, t, t, 1);
    }
    
    private Vector4 Shade(Vector3 point, Vector3 normal, Vector4 color)
    {
        Vector3 lightAccum = new Vector3(0, 0, 0);
        
        for (int i = 0; i < Lights.Length; i++)
        {
            LightSource.ShaderRepresentation light = Lights[i];

            Vector3 shadowRayOrigin;
            Vector3 shadowRayDir;
            Vector3 lightAccumTemp;
            
            switch (light.Type)
            {
                case 0:
                    lightAccumTemp = LightAccumPoint(point, normal, color, light);
                    
                    shadowRayDir = light.Position - point;
                    shadowRayDir = ShaderRotationMethods.Normalize(shadowRayDir);
                    shadowRayOrigin = point + normal * 0.001f;
                    lightAccumTemp *= Shadow(shadowRayOrigin, shadowRayDir, 100, 64);
                    
                    lightAccum += lightAccumTemp;
                    break;
                case 1:
                    lightAccumTemp = LightAccumDirectional(point + normal * 0.001f, normal , color, light);
                    
                    shadowRayDir = new Vector3(0, 0, 1);
                    shadowRayDir = ShaderRotationMethods.Transform(shadowRayDir, light.Rotation);
                    shadowRayOrigin = point + normal * 0.001f;
                    lightAccumTemp *= Shadow(shadowRayOrigin, shadowRayDir, 100, 16);
                    
                    lightAccum += lightAccumTemp;
                    break;
            }
        }
        
        Vector3 colorVec = new Vector3();
        colorVec.X = color.X;
        colorVec.Y = color.Y;
        colorVec.Z = color.Z;
        
        // ambient light
        lightAccum += colorVec * 0.1f;
        
        Vector4 finalColor = new Vector4();
        finalColor.X = lightAccum.X;
        finalColor.Y = lightAccum.Y;
        finalColor.Z = lightAccum.Z;
        finalColor.W = 1;
        
        return finalColor;
    }
    
    private Vector3 LightAccumPoint(Vector3 point, Vector3 normal, Vector4 color, LightSource.ShaderRepresentation light)
    {
        Vector3 lightAccum = new Vector3(0, 0, 0);
        Vector3 colorVec = new Vector3();
        colorVec.X = color.X;
        colorVec.Y = color.Y;
        colorVec.Z = color.Z;
        
        Vector3 lightDir = light.Position - point;
        lightDir = ShaderRotationMethods.Normalize(lightDir);
        
        float diffuse = MathF.Max(0, Vector3.Dot(normal, lightDir));
        
        lightAccum += colorVec * light.Color * diffuse * light.Intensity;

        return lightAccum;
    }
    
    private Vector3 LightAccumDirectional(Vector3 point, Vector3 normal, Vector4 color, LightSource.ShaderRepresentation light)
    {
        Vector3 lightAccum = new Vector3(0, 0, 0);
        Vector3 colorVec = new Vector3();
        colorVec.X = color.X;
        colorVec.Y = color.Y;
        colorVec.Z = color.Z;
        
        Vector4 lightRot = light.Rotation;
        
        Vector3 lightDir = new Vector3(0, 0, 1);
        lightDir = ShaderRotationMethods.Transform(lightDir, lightRot);
        
        float diffuse = MathF.Max(0, Vector3.Dot(normal, lightDir));
        
        lightAccum += colorVec * light.Color * diffuse * light.Intensity;
        
        return lightAccum;
    }
    
    private float Shadow(Vector3 rayOrigin, Vector3 rayDir, float tMax, float k)
    {
        float res = 1;
        float t = 0;
        for (int i = 0; i < MaxIterations && t<tMax; i++)
        {
            float h = GetDistScene(rayOrigin + rayDir * t);
            if (h < 0.001f)
            {
                return 0;
            }
            res = MathF.Min(res, k * h / t);
            t += h;
        }
        
        return res;
    }
    
    // SDF functions
    
    private float GetDistScene(Vector3 point)
    {
        float minDistance = float.MaxValue;

        for (int i = 0; i < Objects.Length; i++)
        {
            GameObject.ShaderRepresentation obj = Objects[i];
            int sdfType = obj.SdfType;

            float distance = GetDist(point, obj.Position, obj.Scale, obj.Rotation, sdfType);
            
            // dont allow negative distances
            if (distance < 0)
            {
                distance = float.MaxValue;
            }

            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }

    private float GetDist(Vector3 point, Vector3 center, Vector3 size, Vector4 rotation, int sdfType)
    {
        switch (sdfType)
        {
            case 0:
                return SphereSdf(point, center, size.X);
            case 1:
                return BoxSdf(point, center, size, rotation);
            case 2:
                return PlaneSdf(point, center, size, rotation);
        }

        return 0;
    }
    
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

        // get the distance from the point to the box
        Vector3 q = Abs(point) - size;
        float d = Length(Max(q, new Vector3(0, 0, 0))) + MathF.Min(MathF.Max(q.X, MathF.Max(q.Y, q.Z)), 0);
        
        // smooth the edges
        float k = 0.01f;
        return d - k;
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

    private Vector3 GetNormal(Vector3 point, Vector3 center, Vector3 size, Vector4 rotation, int sdfType)
    {
        float h = 0.001f;
        Vector2 k = new Vector2(1, -1);
        Vector3 normal = new Vector3();
        
        Vector3 kxyy = new Vector3(k.X, k.Y, k.Y);
        Vector3 kyyx = new Vector3(k.Y, k.Y, k.X);
        Vector3 kyxy = new Vector3(k.Y, k.X, k.Y);
        Vector3 kxxx = new Vector3(k.X, k.X, k.X);
        
        normal += kxyy * GetDist(point + kxyy * h, center, size, rotation, sdfType);
        normal += kyyx * GetDist(point + kyyx * h, center, size, rotation, sdfType);
        normal += kyxy * GetDist(point + kyxy * h, center, size, rotation, sdfType);
        normal += kxxx * GetDist(point + kxxx * h, center, size, rotation, sdfType);
        
        // fix box normals
        
        
        return ShaderRotationMethods.Normalize(normal);
    }
    
    // utility functions
    
    [ShaderMethod]
    private static Vector3 Clamp(Vector3 x, Vector3 min, Vector3 max)
    {
        return new Vector3(Math.Clamp(x.X, min.X, max.X), Math.Clamp(x.Y, min.Y, max.Y), Math.Clamp(x.Z, min.Z, max.Z));
    }
    
    [ShaderMethod]
    private static Vector3 Smoothstep(Vector3 edge0, Vector3 edge1, Vector3 x)
    {
        Vector3 t = Clamp((x - edge0) / (edge1 - edge0), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
        return t * t * (new Vector3(3, 3, 3) - new Vector3(2, 2, 2) * t);
    }
    
    [ShaderMethod]
    private static Vector3 MulByScalar(Vector3 v, float s)
    {
        return new Vector3(v.X * s, v.Y * s, v.Z * s);
    }
    
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