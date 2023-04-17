using System.Drawing;
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
    public readonly ReadWriteBuffer<PathwayLight.ShaderRepresentation> Lights;

    public readonly Camera.ShaderRepresentation Camera;

    #region Dispatch

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
        
        outColor = SmoothClampColor(outColor);

        int outR = (int)Math.Clamp(outColor.X * 255, 0, 255);
        int outG = (int)Math.Clamp(outColor.Y * 255, 0, 255);
        int outB = (int)Math.Clamp(outColor.Z * 255, 0, 255);
        int outA = 255;
        int outInt = outA << 24 | outB << 16 | outG << 8 | outR;

        Buffer[ThreadIds.X] = outInt;
    }

    #endregion

    #region Ray Marching

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
                var obj = Objects[j];
                float objDistance = ObjectSdf(point, Objects[j]);
                distance = MathF.Min(distance, objDistance);

                // dont allow negative distances
                if (distance < 0)
                {
                    distance = float.MaxValue;
                }

                if (objDistance == distance)
                {
                    closestObject = obj;

                    if (distance < 0.001f)
                    {
                        // we hit an object
                        Vector3 normal = GetNormal(point, obj.Position, obj.Scale, obj.Rotation, obj.ObjectType);
                        return Shade(point, normal,obj);
                    }
                }
            }

            t += distance;
        }

        return hitColor;
    }

    #endregion

    #region SDF functions
    
    private float SceneSdf(Vector3 point)
    {
        float distance = float.MaxValue;

        for (int i = 0; i < Objects.Length; i++)
        {
            float objDistance = ObjectSdf(point, Objects[i]);
            distance = MathF.Min(distance, objDistance);
        }

        return distance;
    }

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
        float k = 0.1f;
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

        return ShaderMath.Normalize(normal);
    }

    #endregion

    #region Shading functions

    private Vector3 Shade(Vector3 point, Vector3 normal, PathwayObject.ShaderRepresentation obj)
    {
        Vector3 lightAccum = new Vector3(0, 0, 0);
        
        for (int i = 0; i < Lights.Length; i++)
        {
            PathwayLight.ShaderRepresentation light = Lights[i];
            
            Vector3 shadowRayOrigin;
            Vector3 shadowRayDir;
            Vector3 lightAccumTemp;
            
            switch (light.Type)
            {
                // point light
                case 0:
                    lightAccumTemp = LightAccumPoint(point, normal, obj, light);
                    
                    shadowRayDir = light.Position - point;
                    shadowRayDir = ShaderMath.Normalize(shadowRayDir);
                    shadowRayOrigin = point + normal * 0.001f;
                    
                    lightAccumTemp *= Shadow(shadowRayOrigin, shadowRayDir, 100, 64);
                    
                    lightAccum += lightAccumTemp;
                    break;
                // directional light
                case 1:
                    lightAccumTemp = LightAccumDirectional(point + normal * 0.001f, normal , obj, light);
                    
                    shadowRayDir = new Vector3(0, 0, 1);
                    shadowRayDir = ShaderMath.Transform(shadowRayDir, light.Rotation);
                    shadowRayOrigin = point + normal * 0.001f;
                    
                    lightAccumTemp *= Shadow(shadowRayOrigin, shadowRayDir, 100, 16);
                    
                    lightAccum += lightAccumTemp;
                    break;
            }
        }

        // ambient light
        lightAccum += obj.Color * 0.1f;
        
        return lightAccum;
    }
    
    private Vector3 LightAccumPoint(Vector3 point, Vector3 normal, PathwayObject.ShaderRepresentation obj, PathwayLight.ShaderRepresentation light)
    {
        Vector3 lightAccum = new Vector3(0, 0, 0);
        
        Vector3 lightDir = light.Position - point;
        lightDir = ShaderMath.Normalize(lightDir);
        
        // calculate the diffuse light
        float diffuse = MathF.Max(0, Vector3.Dot(normal, lightDir));
        
        // calculate the specular light
        Vector3 viewDir = Camera.Position - point;
        viewDir = ShaderMath.Normalize(viewDir);
        Vector3 halfwayDir = ShaderMath.Normalize(lightDir + viewDir);
        
        float specular = MathF.Pow(MathF.Max(0, Vector3.Dot(normal, halfwayDir)), obj.Smoothness * 64f);
        
        lightAccum += obj.Color * light.Color * (diffuse + specular * obj.Smoothness) * light.Intensity;

        return lightAccum;
    }
    
    private Vector3 LightAccumDirectional(Vector3 point, Vector3 normal, PathwayObject.ShaderRepresentation obj, PathwayLight.ShaderRepresentation light)
    {
        Vector3 lightAccum = new Vector3(0, 0, 0);
        
        Vector4 lightRot = light.Rotation;
        
        Vector3 lightDir = new Vector3(0, 0, 1);
        lightDir = ShaderMath.Transform(lightDir, lightRot);
        
        // calculate the diffuse light
        float diffuse = MathF.Max(0, Vector3.Dot(normal, lightDir));
        
        // calculate the specular light
        Vector3 viewDir = Camera.Position - point;
        viewDir = ShaderMath.Normalize(viewDir);
        Vector3 halfwayDir = ShaderMath.Normalize(lightDir + viewDir);
        
        float specular = MathF.Pow(MathF.Max(0, Vector3.Dot(normal, halfwayDir)), obj.Smoothness * 64f);
        
        lightAccum += obj.Color * light.Color * (diffuse + specular * obj.Smoothness) * light.Intensity;
        
        return lightAccum;
    }
    
    private float Shadow(Vector3 rayOrigin, Vector3 rayDir, float tMax, float k)
    {
        float res = 1;
        float t = 0;
        for (int i = 0; i < MaxSteps && t<tMax; i++)
        {
            float h = SceneSdf(rayOrigin + rayDir * t);
            if (h < 0.001f)
            {
                return 0;
            }
            res = MathF.Min(res, k * h / t);
            t += h;
        }
        
        return res;
    }

    public Vector3 SmoothClampColor(Vector3 color)
    {
        float a = 3f;
        
        float r = 1 / (1 + MathF.Exp(-a * color.X));
        r = r * 2 - 1;
        float g = 1 / (1 + MathF.Exp(-a * color.Y));
        g = g * 2 - 1;
        float b = 1 / (1 + MathF.Exp(-a * color.Z));
        b = b * 2 - 1;
        
        return new Vector3(r, g, b);
    }

    #endregion
}