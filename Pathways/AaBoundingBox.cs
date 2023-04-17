using System.Numerics;

namespace Pathways;

public struct AaBoundingBox
{
    public Vector3 LowerBound;
    public Vector3 UpperBound;
    
    public AaBoundingBox(Vector3 lowerBound, Vector3 upperBound)
    {
        LowerBound = lowerBound;
        UpperBound = upperBound;
    }

    public bool RayIntersects(Vector3 rayOrgin, Vector3 rayDir)
    {
        Vector3 vt1 = (LowerBound - rayOrgin) / rayDir;
        Vector3 vt2 = (UpperBound - rayOrgin) / rayDir;
        Vector3 tmin = Vector3.Min(vt1, vt2);
        Vector3 tmax = Vector3.Max(vt1, vt2);
        float t0 = MathF.Max(tmin.X, MathF.Max(tmin.Y, tmin.Z));
        float t1 = MathF.Min(tmax.X, MathF.Min(tmax.Y, tmax.Z));
        return t0 < t1;
    }
}