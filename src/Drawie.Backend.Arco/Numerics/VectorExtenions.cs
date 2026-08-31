using System.Numerics;
using Drawie.Numerics;

namespace Drawie.Backend.Arco.Numerics;

public static class VectorExtenions
{
    public static Vector2 ToVector2(this VecD vecD)
    {
        return new Vector2((float)vecD.X, (float)vecD.Y);
    }
    
    public static Vector2 ToVector2(this VecI vecD)
    {
        return new Vector2(vecD.X, vecD.Y);
    }
}