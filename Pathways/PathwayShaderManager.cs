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
        GraphicsDevice.GetDefault().For(ShaderBuffer.Length,
            new RayMarchSahder(ShaderBuffer, TargetTexture.texture.width, TargetTexture.texture.height));

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