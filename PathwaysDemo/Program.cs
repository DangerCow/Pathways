using Pathways;
using System.Numerics;

namespace PathwaysDemo;

public class Program
{
    public static readonly Vector2 WindowSize = new Vector2(1280, 720);
    
    public static void Main()
    {
        var pathway = new Pathway("Pathways Demo", WindowSize);
        pathway.StartGameLoop();
    }
}