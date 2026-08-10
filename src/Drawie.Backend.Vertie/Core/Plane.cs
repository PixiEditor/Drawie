using System.Numerics;
using Drawie.Numerics;

namespace Drawie.Backend.Vertie;

public struct Plane
{
    public Vec3D Normal { get; set; }
    public Vec3D Point { get; set; }

    public double GetSignedDistance(Vec3D point)
    {
        return Normal.Dot(point - Point);
    }

    public Plane(Vec3D point, Vec3D normal)
    {
        Normal = normal.Normalize();
        Point = point;
    }
}