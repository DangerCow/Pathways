using Pathways;
using Raylib_cs;
using System.Numerics;

namespace PathwaysDemo;

public class BouncySphere : GameObject
{
    public Vector3 Velocity = new Vector3(0, 4, 0);
    
    public BouncySphere()
    {
        Sdf = SdfType.Sphere;
        Color = Color.RED;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        
        Position += Velocity * deltaTime;
        
        if (Position.Y < 1)
        {
            Position = new Vector3(Position.X, 1, Position.Z);
            Velocity = new Vector3(Velocity.X, -Velocity.Y, Velocity.Z);
        }
        else if (Position.Y > 4)
        {
            Position = new Vector3(Position.X, 4, Position.Z);
            Velocity = new Vector3(Velocity.X, -Velocity.Y, Velocity.Z);
        }
    }
    
    public class BouncyCube : BouncySphere
    {
        public BouncyCube()
        {
            Rotation = new Rotation(new Vector3(0, 45, 0));
            Velocity = new Vector3(0, 2, 0);
            Sdf = SdfType.Box;
            Color = Color.BLUE;
        }
    }
}