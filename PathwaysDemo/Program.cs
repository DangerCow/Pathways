using Pathways;
using System.Numerics;
using Raylib_cs;

namespace PathwaysDemo;

public class DemoScene : Scene
{
    public override void Init()
    {
        Camera = new Camera(new Vector3(0, 6, 10), new Rotation(new Vector3(-30, 0, 0)), 60);

        PathwayObject cube = new PathwayObject();
        cube.Position = new Vector3(0, 1, 0);
        cube.Rotation = new Rotation(new Vector3(0, 45, 0));
        cube.Scale = new Vector3(1, 1, 1);
        cube.Color = Color.RED;
        cube.Type = PathwayObject.ObjectType.Box;
        
        PathwayObject plane = new PathwayObject();
        plane.Position = new Vector3(0, 0, 0);
        plane.Rotation = new Rotation(new Vector3(0, 0, 0));
        plane.Scale = new Vector3(10, 1, 10);
        plane.Color = Color.WHITE;
        plane.Type = PathwayObject.ObjectType.Plane;
        
        PathwayObject sphere = new PathwayObject();
        sphere.Position = new Vector3(0, 3, 0);
        sphere.Rotation = new Rotation(new Vector3(0, 0, 0));
        sphere.Scale = new Vector3(1, 1, 1);
        sphere.Color = Color.BLUE;
        sphere.Smoothness = 1f;
        sphere.Type = PathwayObject.ObjectType.Sphere;
        
        
        Objects.Add(cube);
        Objects.Add(plane);
        Objects.Add(sphere);
        
        PathwayLight light = new PathwayLight();
        light.Position = new Vector3(2, 8, 8);
        light.Color = Color.WHITE;
        light.Intensity = 1;
        light.Type = PathwayLight.LightType.Point;
        
        PathwayLight light2 = new PathwayLight();
        light2.Position = new Vector3(-5, 5, 5);
        light2.Color = Color.BLUE;
        light2.Intensity = 0.5f;
        light2.Type = PathwayLight.LightType.Point;
        
        Lights.Add(light);
        Lights.Add(light2);
    }

    public override void Update()
    {
        base.Update();
        
        Camera.FlyControlls(5, 2, Raylib.GetFrameTime());
    }
    
    public override void Draw2D()
    {
        base.Draw2D();
        Raylib.DrawText($"Camera Position: {Camera.Position}", 10, 40, 1, Color.WHITE);
        Raylib.DrawText($"Camera Rotation: {Camera.Rotation.ToString()}", 10, 50, 1, Color.WHITE);
    }
}

public class Program
{
    public static readonly Vector2 WindowSize = new Vector2(1280, 720);

    public static void Main()
    {
        var pathway = new Pathway("Pathways Demo", WindowSize, new DemoScene());
        pathway.StartGameLoop();
    }
}