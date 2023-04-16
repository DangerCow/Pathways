using System.Numerics;
using Pathways;
using Raylib_cs;

namespace PathwaysDemo;

public static class Program
{
    public class DemoScene : Scene
    {
        float _time = 0;
        
        public override void Init()
        {
            base.Init();
            
            SceneCamera.Position = new Vector3(0, 4, 10);
            SceneCamera.Rotation = new Rotation(new Vector3(-30, 0, 0));

            LightSource Sun = new LightSource();
            Sun.Position = new Vector3(0, 10, 0);
            Sun.Color = Color.WHITE;
            Sun.Rotation = new Rotation(new Vector3(-70, 15, 0));
            Sun.Type = LightSource.LightType.Directional;
            AddLightSource(Sun);

            GameObject sphere = new BouncySphere();
            sphere.Position = new Vector3(1.5f, 2, 0);
            AddGameObject(sphere);
            
            GameObject cube = new BouncySphere.BouncyCube();
            cube.Position = new Vector3(-1.5f, 2, 0);
            AddGameObject(cube);
            
            GameObject plane = new GameObject();
            plane.Sdf = GameObject.SdfType.Plane;
            plane.Scale = new Vector3(10, 1, 10);
            plane.Position = new Vector3(0, 0, 0);
            plane.Color = Color.SKYBLUE;
            AddGameObject(plane);
        }
        
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            _time += deltaTime;
            
            GameObjects[2].Rotation = new Rotation(new Vector3(0, _time * 10, 0));
        }

        public override void Draw2D()
        {
            Raylib.DrawText($"Camera position: {SceneCamera.Position}", 10, 30, 20, Color.GREEN);
            Raylib.DrawText($"Camera rotation: {SceneCamera.Rotation.ToString()}", 10, 50, 20, Color.GREEN);
            Raylib.DrawFPS(10, 10);
        }
    }

    public static void Main()
    {
        Scene demoScene = new DemoScene();

        Window window = new(960, 720, "Pathways Demo", demoScene);
        window.StartGameLoop();
    }
}