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
        public float Smoothness;
        
        public AaBoundingBox BoundingBox;
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
    public float Smoothness = 0.5f;

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
    
    public AaBoundingBox GetBoundingBox()
    {
        // Scale the bounding box by 1.5 to make sure it's big enough
        Vector3 scale = Scale * 1.5f;

        Vector3 lowerBound = Position - scale;
        Vector3 upperBound = Position + scale;
        
        // Apply rotation
        Vector3[] corners = new Vector3[8];
        corners[0] = lowerBound;
        corners[1] = new Vector3(lowerBound.X, lowerBound.Y, upperBound.Z);
        corners[2] = new Vector3(lowerBound.X, upperBound.Y, lowerBound.Z);
        corners[3] = new Vector3(lowerBound.X, upperBound.Y, upperBound.Z);
        corners[4] = new Vector3(upperBound.X, lowerBound.Y, lowerBound.Z);
        corners[5] = new Vector3(upperBound.X, lowerBound.Y, upperBound.Z);
        corners[6] = new Vector3(upperBound.X, upperBound.Y, lowerBound.Z);
        corners[7] = upperBound;
        
        Vector3 min = new Vector3(float.MaxValue);
        Vector3 max = new Vector3(float.MinValue);
        
        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 transformed = Vector3.Transform(corners[i], Rotation.Value);
            min = Vector3.Min(min, transformed);
            max = Vector3.Max(max, transformed);
        }
        
        lowerBound = min;
        upperBound = max;

        return new AaBoundingBox(lowerBound, upperBound);
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
        Vector3 color = new Vector3(Color.r / 255f, Color.g / 255f, Color.b / 255f);

        return new ShaderRepresentation
        {
            Position = Position,
            Rotation = Rotation.ToVector4(),
            Scale = Scale,
            ObjectType = (int)Type,
            Color = color,
            Smoothness = Smoothness,
            BoundingBox = GetBoundingBox()
        };
    }
}