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
            
            float distance = SceneSDF(point);
            
            if (distance < 0.001f)
            {
                hitColor = new Vector3(1, 0, 0);
                break;
            }
            
            t += distance;
        }
        
        return hitColor;
    }
    
    // SDF FUNCTIONS

    private float SceneSDF(Vector3 point)
    {
        float distance = float.MaxValue;
        
        for (int i = 0; i < Objects.Length; i++)
        {
            var obj = Objects[i];

            switch (obj.ObjectType)
            {
                // Sphere
                case 0:
                    distance = MathF.Min(distance, SphereSDF(point, obj.Position, obj.Scale.X));
                    break;
            }
        }
        
        return distance;
    }
    
    private float SphereSDF(Vector3 point, Vector3 center, float radius)
    {
        return ShaderMath.Length(point - center) - radius;
    }
}