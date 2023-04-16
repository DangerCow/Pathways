using Raylib_cs;
using System.Numerics;

namespace Pathways;

public class PathwayLight
{
    public struct ShaderRepresentation
    {
        public Vector3 Position;
        public Vector3 Color;
        public Vector4 Rotation;
        public float Intensity;
        public float ShadowSharpness;
        public int Type;
    }
    
    public enum LightType
    {
        Point,
        Directional,
    }
    
    public Vector3 Position;
    public Color Color;
    public Rotation Rotation;
    public float Intensity;
    public float ShadowSharpness = 32;
    
    public LightType Type;
    
    public PathwayLight(Vector3 position, Color color, Rotation rotation, float intensity, LightType type)
    {
        Position = position;
        Color = color;
        Rotation = rotation;
        Intensity = intensity;
        Type = type;
    }
    
    public PathwayLight()
    {
        Position = new Vector3();
        Color = Color.WHITE;
        Rotation = new Rotation();
        Intensity = 1;
        Type = LightType.Point;
    }
    
    public ShaderRepresentation GetShaderRepresentation()
    {
        Vector4 Rotation;
        Rotation.X = this.Rotation.Value.X;
        Rotation.Y = this.Rotation.Value.Y;
        Rotation.Z = this.Rotation.Value.Z;
        Rotation.W = this.Rotation.Value.W;
        
        Vector3 Color = new Vector3();
        Color.X = this.Color.r / 255f;
        Color.Y = this.Color.g / 255f;
        Color.Z = this.Color.b / 255f;

        return new ShaderRepresentation
        {
            Position = Position,
            Color = Color,
            Rotation = Rotation,
            Intensity = Intensity,
            ShadowSharpness = ShadowSharpness,
            Type = (int)Type
        };
    }
}