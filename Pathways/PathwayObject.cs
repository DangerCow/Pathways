using ComputeSharp;
using Raylib_cs;
using System.Numerics;

namespace Pathways;

public class PathwayObject
{
    public struct ShaderRepresentation
    {
        public Vector3 Position;
        public Vector4 Rotation;
        public Vector3 Scale;

        public int ObjectType;

        public Vector3 Color;
    }

    public enum ObjectType
    {
        Sphere,
        Box,
        Plane,
    }

    public Vector3 Position;
    public Rotation Rotation;
    public Vector3 Scale;

    public ObjectType Type;

    public Color Color;

    public PathwayObject(Vector3 position, Rotation rotation, Vector3 scale, ObjectType type, Color color)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Type = type;
        Color = color;
    }

    public PathwayObject()
    {
        Position = Vector3.Zero;
        Rotation = new Rotation();
        Scale = Vector3.One;
        Type = ObjectType.Sphere;
        Color = Color.WHITE;
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
        Vector3 color = new Vector3(Color.r, Color.g, Color.b);

        return new ShaderRepresentation
        {
            Position = Position,
            Rotation = Rotation.ToVector4(),
            Scale = Scale,
            ObjectType = (int)Type,
            Color = color
        };
    }
}