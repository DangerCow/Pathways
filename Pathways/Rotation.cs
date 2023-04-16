using System.Numerics;

namespace Pathways;

public class Rotation
{
    public Quaternion Value { get; set; }
    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Value);
    public Vector3 Right => Vector3.Transform(Vector3.UnitX, Value);
    public Vector3 Forward => Vector3.Transform(-Vector3.UnitZ, Value);

    public Rotation(Quaternion value)
    {
        Value = value;
    }

    public Rotation(Vector3 euler)
    {
        euler *= (float)(Math.PI / 180);
        Value = Quaternion.CreateFromYawPitchRoll(euler.Y, euler.X, euler.Z);
    }

    public Rotation()
    {
        Value = Quaternion.Identity;
    }

    public static Rotation operator *(Rotation a, Rotation b)
    {
        return new Rotation(a.Value * b.Value);
    }

    public static Rotation operator *(Rotation a, float b)
    {
        return new Rotation(a.Value * b);
    }

    public static Rotation operator *(float a, Rotation b)
    {
        return new Rotation(b.Value * a);
    }

    public static Rotation operator +(Rotation a, Rotation b)
    {
        return new Rotation(a.Value + b.Value);
    }

    public static Rotation operator -(Rotation a, Rotation b)
    {
        return new Rotation(a.Value - b.Value);
    }

    public Vector3 YawPitchRoll
    {
        get
        {
            Vector3 euler;
            Quaternion q = Value;

            float sqw = q.W * q.W;
            float sqy = q.Y * q.Y;
            float sqz = q.Z * q.Z;

            euler.Y = (float)Math.Atan2(2f * q.X * q.W + 2f * q.Y * q.Z, 1 - 2f * (sqz + sqw)); // Yaw
            euler.X = (float)Math.Asin(2f * (q.X * q.Z - q.W * q.Y)); // Pitch
            euler.Z = (float)Math.Atan2(2f * q.X * q.Y + 2f * q.Z * q.W, 1 - 2f * (sqy + sqz)); // Roll

            euler *= (float)(180 / Math.PI);
            return euler;
        }
        set
        {
            value *= (float)(Math.PI / 180);
            Value = Quaternion.CreateFromYawPitchRoll(value.Y, value.X, value.Z);

            Console.WriteLine($"Yaw: {value.Y}, Pitch: {value.X}, Roll: {value.Z}");
        }
    }

    public void Rotate(Vector3 euler)
    {
        euler *= (float)(Math.PI / 180);
        Value = Quaternion.CreateFromYawPitchRoll(euler.Y, euler.X, euler.Z) * Value;
    }

    public void Rotate(Quaternion q)
    {
        Value = q * Value;
    }
    
    public string ToString()
    {
        return $"Yaw: {YawPitchRoll.Y}, Pitch: {YawPitchRoll.X}, Roll: {YawPitchRoll.Z}";
    }
    
    public Vector4 ToVector4()
    {
        return new Vector4(Value.X, Value.Y, Value.Z, Value.W);
    }
}