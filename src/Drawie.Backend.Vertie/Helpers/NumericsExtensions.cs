using System.Numerics;
using Drawie.Numerics;

namespace Drawie.Backend.Vertie;

public static class NumericsExtensions
{
    public static Vector3 ToVector3(this Vec3D vec)
    {
        return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
    }
}