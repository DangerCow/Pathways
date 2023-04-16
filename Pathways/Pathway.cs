using System.Numerics;
using Raylib_cs;

namespace Pathways;

public class Pathway
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
    
    internal PathwayShaderManager ShaderManager = new PathwayShaderManager();

    public Vector2 WindowWindowSize
    {
        get => new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        set
        {
            Raylib.SetWindowSize((int)value.X, (int)value.Y);
            ShaderManager.UpdateBuffer(value);
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

    public Pathway(string title, Vector2 windowSize, Scene scene)
    {
        Raylib.InitWindow((int)windowSize.X, (int)windowSize.Y, title);
        Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT);
        Title = title;
        WindowWindowSize = windowSize;
        Scene = scene;
    }
    
    public Pathway(string title, Vector2 windowSize)
    {
        Raylib.InitWindow((int)windowSize.X, (int)windowSize.Y, title);
        Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT);
        Title = title;
        WindowWindowSize = windowSize;
        Scene = new Scene();
    }
    
    public void StartGameLoop()
    {
        while (IsOpen)
        {
            Update();
            Draw();
        }
    }
    
    public virtual void Update(){}

    public virtual void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.BLACK);
        
        Raylib.DrawTexture(ShaderManager.Render(_scene).texture, 0, 0, Color.WHITE);
        
        // TODO: Remove debug fps indicator
        Raylib.DrawFPS(10, 10);
        Raylib.EndDrawing();
    }
}