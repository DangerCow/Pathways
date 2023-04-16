using System.Numerics;
using ComputeSharp;
using Raylib_cs;
using Pathways.Shaders;

namespace Pathways;

public class PathwayShaderManager
{
    internal ReadWriteBuffer<int>? ShaderBuffer { get; private set; }
    internal RenderTexture2D TargetTexture { get; private set; }

    internal void UpdateBuffer(Vector2 size)
    {
        if (size.Length() == 0)
            return;
        ShaderBuffer?.Dispose();

        ShaderBuffer = GraphicsDevice.GetDefault().AllocateReadWriteBuffer<int>((int)size.X * (int)size.Y);
        TargetTexture = Raylib.LoadRenderTexture((int)size.X, (int)size.Y);
    }

    internal RenderTexture2D Render(Scene scene)
    {
        if (scene == null || scene.Objects.Count == 0)
            return TargetTexture;
        
        //get objects
        var objects = scene.GetObjectsShaderRepresentation();
        ReadWriteBuffer<PathwayObject.ShaderRepresentation> objectsBuffer = GraphicsDevice.GetDefault()
            .AllocateReadWriteBuffer<PathwayObject.ShaderRepresentation>(objects.Length);
        objectsBuffer.CopyFrom(objects);

        //get the camera
        var camera = scene.Camera.GetShaderRepresentation();

        GraphicsDevice.GetDefault().For(ShaderBuffer.Length,
            new RayMarchShader(
                ShaderBuffer,
                TargetTexture.texture.width,
                TargetTexture.texture.height,
                objectsBuffer,
                camera));

        unsafe
        {
            fixed (int* ptr = ShaderBuffer.ToArray())
            {
                Raylib.UpdateTexture(TargetTexture.texture, ptr);
            }
        }

        return TargetTexture;
    }
}