using Raylib_cs;
using System.Numerics;

namespace Pathways;

public class Camera
{
    public struct ShaderRepresentation
    {
        public Vector3 Position;
        public Vector3 Target;
        public Vector3 Up;
        public Vector4 Rotation;
        public float Fov;
    }

    private Camera3D _raylibCamera;
    private Rotation _rotation;

    public Vector3 Position
    {
        get => _raylibCamera.position;
        set => _raylibCamera.position = value;
    }

    public Vector3 Target
    {
        get => _raylibCamera.target;
        set => _raylibCamera.target = value;
    }

    public Rotation Rotation
    {
        get { return _rotation; }
        set
        {
            _rotation = value;
            _raylibCamera.target = Position + _rotation.Forward;
            _raylibCamera.up = _rotation.Up;
        }
    }

    public float Fov
    {
        get => _raylibCamera.fovy;
        set => _raylibCamera.fovy = value;
    }

    public Camera(Vector3 position, Rotation rotation, float fov)
    {
        _raylibCamera = new Camera3D
        {
            position = position,
            target = position + rotation.Forward,
            up = rotation.Up,
            fovy = fov,
            projection = CameraProjection.CAMERA_PERSPECTIVE
        };
        _rotation = rotation;
    }

    public Camera()
    {
        _raylibCamera = new Camera3D();
        _raylibCamera.position = new Vector3(0, 0, 0);
        _raylibCamera.target = new Vector3(0, 0, 1);
        _raylibCamera.up = new Vector3(0, 1, 0);
        _raylibCamera.fovy = 90;
        _raylibCamera.projection = CameraProjection.CAMERA_PERSPECTIVE;
        _rotation = new Rotation(new Vector3(0, 0, 0));
    }

    public void StartDraw()
    {
        _raylibCamera.target = Position + _rotation.Forward;
        _raylibCamera.up = _rotation.Up;
        Raylib.BeginMode3D(_raylibCamera);
    }

    public void EndDraw()
    {
        Raylib.EndMode3D();
    }

    public ShaderRepresentation GetShaderRepresentation()
    {
        Vector4 Rotation = new Vector4();
        Rotation.X = this.Rotation.Value.X;
        Rotation.Y = this.Rotation.Value.Y;
        Rotation.Z = this.Rotation.Value.Z;
        Rotation.W = this.Rotation.Value.W;

        ShaderRepresentation shaderRepresentation = new ShaderRepresentation();
        shaderRepresentation.Position = this.Position;
        shaderRepresentation.Target = this.Target;
        shaderRepresentation.Up = this._raylibCamera.up;
        shaderRepresentation.Rotation = Rotation;
        shaderRepresentation.Fov = this.Fov;

        return shaderRepresentation;
    }

    public void FlyControlls(float speed, float rotSpeed, float dt)
    {
        speed *= dt;
        rotSpeed *= dt;

        if (Raylib.IsKeyDown(KeyboardKey.KEY_W))
        {
            Position += Rotation.Forward * speed;
        }

        if (Raylib.IsKeyDown(KeyboardKey.KEY_S))
        {
            Position -= Rotation.Forward * speed;
        }

        if (Raylib.IsKeyDown(KeyboardKey.KEY_A))
        {
            Position -= Rotation.Right * speed;
        }

        if (Raylib.IsKeyDown(KeyboardKey.KEY_D))
        {
            Position += Rotation.Right * speed;
        }

        if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
        {
            Position += Rotation.Up * speed;
        }

        if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL))
        {
            Position -= Rotation.Up * speed;
        }

        if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
        {
            Rotation.Rotate(
                Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), rotSpeed * 1.5f)
            );
        }

        if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
        {
            Rotation.Rotate(
                Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), -rotSpeed * 1.5f)
            );
        }

        if (Raylib.IsKeyDown(KeyboardKey.KEY_UP))
        {
            Rotation.Rotate(
                Quaternion.CreateFromAxisAngle(Rotation.Right, rotSpeed)
            );
        }

        if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
        {
            Rotation.Rotate(
                Quaternion.CreateFromAxisAngle(Rotation.Right, -rotSpeed)
            );
        }
    }
}