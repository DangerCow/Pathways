using Raylib_cs;

namespace Pathways;

public class Scene
{
    public Camera Camera = new Camera();
    public List<PathwayObject> Objects = new List<PathwayObject>();
    public List<PathwayLight> Lights = new List<PathwayLight>();

    public virtual void Init()
    {
        foreach (var obj in Objects)
        {
            obj.Init();
        }
    }

    public virtual void Update()
    {
        float deltaTime = Raylib.GetFrameTime();
        
        foreach (var obj in Objects)
        {
            obj.Update(deltaTime);
        }
    }

    public void DoDraw()
    {
        Camera.StartDraw();
        Draw3D();
        Camera.EndDraw();
        Draw2D();
    }

    public virtual void Draw2D()
    {
    }

    public virtual void Draw3D()
    {
        foreach (var obj in Objects)
        {
            obj.Draw();
        }
    }

    public PathwayObject.ShaderRepresentation[] GetObjectsShaderRepresentation()
    {
        var objects = new PathwayObject.ShaderRepresentation[Objects.Count];
        
        foreach (var obj in Objects)
        {
            objects[Objects.IndexOf(obj)] = obj.GetShaderRepresentation();
        }

        return objects;
    }
    
    public PathwayLight.ShaderRepresentation[] GetLightsShaderRepresentation()
    {
        var lights = new PathwayLight.ShaderRepresentation[Lights.Count];
        
        foreach (var light in Lights)
        {
            lights[Lights.IndexOf(light)] = light.GetShaderRepresentation();
        }

        return lights;
    }
}