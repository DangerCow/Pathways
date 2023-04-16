using Raylib_cs;
using System.Numerics;

namespace Pathways;

public class LightSource
{
    public struct ShaderRepresentation
    {
        public Vector3 Position;
        public Vector3 Color;
        public Vector4 Rotation;
        public float Intensity;
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
    
    public LightType Type;
    
    public LightSource(Vector3 position, Color color, Rotation rotation, float intensity, LightType type)
    {
        Position = position;
        Color = color;
        Rotation = rotation;
        Intensity = intensity;
        Type = type;
    }
    
    public LightSource()
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
        Color.X = (float)this.Color.r / 255;
        Color.Y = (float)this.Color.g / 255;
        Color.Z = (float)this.Color.b / 255;
        
        return new ShaderRepresentation
        {
            Position = Position,
            Color = Color,
            Rotation = Rotation,
            Intensity = Intensity,
            Type = (int)Type,
        };
    }
}