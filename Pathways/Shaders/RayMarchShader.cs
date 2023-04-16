using System.Numerics;
using ComputeSharp;

namespace Pathways.Shaders;

[AutoConstructor]
public readonly partial struct RayMarchSahder : IComputeShader
{
    public readonly ReadWriteBuffer<int> Buffer;
    public readonly int Width;
    public readonly int Height;

    public void Execute()
    {
        int x = ThreadIds.X % Width;
        int y = ThreadIds.X / Height;
        
        float u = x / (float)Width;
        float v = y / (float)Height;
        
        Vector3 outColor = new Vector3(u, v, 0);
        
        int outR = (int)Math.Clamp(outColor.X * 255, 0, 255);
        int outG = (int)Math.Clamp(outColor.Y * 255, 0, 255);
        int outB = (int)Math.Clamp(outColor.Z * 255, 0, 255);
        int outA = 255;
        int outInt = outA << 24 | outB << 16 | outG << 8 | outR;
        
        Buffer[ThreadIds.X] = outInt;
    }
}