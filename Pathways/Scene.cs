using Raylib_cs;

namespace Pathways;

public class Scene
{
    public Camera Camera = new Camera();
    public List<PathwayObject> Objects = new List<PathwayObject>();

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
}