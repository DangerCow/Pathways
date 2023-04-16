using System.Numerics;
using ComputeSharp;
using Pathways.Shaders;
using Raylib_cs;

namespace Pathways;

public class Window
{
    private Scene _scene;

    public Scene Scene
    {
        get => _scene;
        set
        {
            _scene = value;
            _scene.Init();
        }
    }

    public int Width
    {
        get => Raylib.GetScreenWidth();
        set
        {
            Raylib.SetWindowSize(value, Height);
            UpdateBuffers();
        }
    }

    public int Height
    {
        get => Raylib.GetScreenHeight();
        set
        {
            Raylib.SetWindowSize(Width, value);
            UpdateBuffers();
        }
    }

    private string _title;

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            Raylib.SetWindowTitle(value);
        }
    }

    public bool IsOpen => !Raylib.WindowShouldClose();

    public Window(int width, int height, string title, Scene initialScene)
    {
        Width = width;
        Height = height;
        Title = title;

        Scene = initialScene;

        Raylib.InitWindow(width, height, title);
        Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT);

        UpdateBuffers();
    }

    public Window(int width, int height, string title)
    {
        Width = width;
        Height = height;
        Title = title;

        Raylib.InitWindow(width, height, title);
        Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT);

        UpdateBuffers();
    }

    public void StartGameLoop()
    {
        while (IsOpen)
        {
            Update();
            Draw();
        }
    }

    public void Update()
    {
        Scene.Update(Raylib.GetFrameTime());
    }

    public void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.BLACK);

        DrawRaymarchShader();
        Scene.DoDraw();

        Raylib.EndDrawing();
    }

    // Compute shader stuff
    private ReadWriteBuffer<Vector4> _outputBuffer;
    private Color[] _outputColorBuffer;
    private RenderTexture2D _renderTexture;

    private void UpdateBuffers()
    {
        if (Width == 0 || Height == 0) return;
        _outputBuffer?.Dispose();
        _outputBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer<Vector4>(Width * Height);
        _renderTexture = Raylib.LoadRenderTexture(Width, Height);
        _outputColorBuffer = new Color[Width * Height];
    }

    private void DrawRaymarchShader()
    {
        if (Scene == null || Scene.GetObjectsShaderRepresentation() == null ||
            Scene.GetObjectsShaderRepresentation().Length == 0)
            return;

        GameObject.ShaderRepresentation[] objectsShaderRepresentation = Scene.GetObjectsShaderRepresentation();
        ReadWriteBuffer<GameObject.ShaderRepresentation> objectsShaderRepresentationBuffer =
            GraphicsDevice.GetDefault().AllocateReadWriteBuffer(objectsShaderRepresentation);

        LightSource.ShaderRepresentation[] lightsShaderRepresentation = Scene.GetLightSourcesShaderRepresentation();
        ReadWriteBuffer<LightSource.ShaderRepresentation> lightsShaderRepresentationBuffer =
            GraphicsDevice.GetDefault().AllocateReadWriteBuffer(lightsShaderRepresentation);

        GraphicsDevice.GetDefault().For(_outputBuffer.Length,
            new RaymarchShader(_outputBuffer, Width, Height, objectsShaderRepresentationBuffer,
                lightsShaderRepresentationBuffer, Scene.SceneCamera.GetShaderRepresentation()));
        Vector4[] data = _outputBuffer.ToArray();

        // Copy the buffer to a color buffer
        Parallel.For(0, _outputColorBuffer.Length, i =>
        {
            Vector4 color = data[i];
            int r = (int)Math.Clamp(color.X * 255, 0, 255);
            int g = (int)Math.Clamp(color.Y * 255, 0, 255);
            int b = (int)Math.Clamp(color.Z * 255, 0, 255);
            int a = (int)Math.Clamp(color.W * 255, 0, 255);
            _outputColorBuffer[i] = new Color(r, g, b, a);
        });

        unsafe
        {
            fixed (Color* ptr = _outputColorBuffer)
            {
                Raylib.UpdateTexture(_renderTexture.texture, ptr);
            }
        }

        Raylib.DrawTexture(_renderTexture.texture, 0, 0, Color.WHITE);
    }
}