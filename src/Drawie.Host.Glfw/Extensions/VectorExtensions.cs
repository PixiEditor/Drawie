using Drawie.Numerics;
using Silk.NET.Maths;

namespace Drawie.Silk.Extensions;

public static class VectorExtensions
{
    public static Vector2D<int> ToVector2DInt(this VecI vec) => new Vector2D<int>(vec.X, vec.Y);
    public static VecI ToVecI(this Vector2D<int> vec) => new VecI(vec.X, vec.Y);
}