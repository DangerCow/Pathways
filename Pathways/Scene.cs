using System.Numerics;
using Raylib_cs;

namespace Pathways;

public class Scene
{
    public List<GameObject> GameObjects = new();
    public Camera SceneCamera = new Camera();

    public void AddGameObject(GameObject gameObject)
    {
        GameObjects.Add(gameObject);
    }
    
    public virtual void Init()
    {
        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Init();
        }
    }

    public virtual void Update(float deltaTime)
    {
        SceneCamera.FlyControlls(4, 1.5f, deltaTime);

        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Update(deltaTime);
        }
    }

    public void DoDraw()
    {
        SceneCamera.StartDraw();
        Draw3D();
        SceneCamera.EndDraw();
        Draw2D();
    }
    
    public virtual void Draw3D()
    {
        foreach (GameObject gameObject in GameObjects)
        {
            gameObject.Draw();
        }
    }
    
    public virtual void Draw2D()
    {
        
    }

    public GameObject.ShaderRepresentation[] GetShaderRepresentations()
    {
        GameObject.ShaderRepresentation[]
            shaderRepresentations = new GameObject.ShaderRepresentation[GameObjects.Count];
        for (int i = 0; i < GameObjects.Count; i++)
        {
            shaderRepresentations[i] = GameObjects[i].GetShaderRepresentation();
        }

        return shaderRepresentations;
    }
}