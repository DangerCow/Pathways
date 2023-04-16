using ComputeSharp;
using Raylib_cs;
using System.Numerics;

namespace Pathways;

public class GameObject
{
    public struct ShaderRepresentation
    {
        public Vector3 Position;
        public Vector4 Rotation;
        public Vector3 Scale;
        
        public int SdfType;
        public Vector3 Color;
    }
    
    public enum SdfType
    {
        Sphere,
        Box,
        Plane,
    }
    
    public Vector3 Position;
    public Rotation Rotation;
    public Vector3 Scale;
    
    public SdfType Sdf;
    
    public Color Color;
    
    public GameObject(Vector3 position, Rotation rotation, Vector3 scale, SdfType sdf, Color color)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Sdf = sdf;
        Color = color;
    }

    public GameObject()
    {
        Position = new Vector3();
        Rotation = new Rotation();
        Scale = new Vector3(1, 1, 1);
        Sdf = SdfType.Sphere;
        Color = Color.RED;
    }
    
    public virtual void Init()
    {
    }
    public virtual void Update(float deltaTime)
    {
    }
    public virtual void Draw()
    {
    }
    
    public ShaderRepresentation GetShaderRepresentation()
    {
        Vector4 Rotation = new Vector4();
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
            Rotation = Rotation,
            Scale = Scale,
            SdfType = (int)Sdf,
            Color = Color
        };
    }
}