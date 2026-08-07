using System.Numerics;

namespace SilkNet.Rendering;

public struct Plane
{
    public Vector3 Normal { get; set; }
    public Vector3 Point { get; set; }

    public float GetSignedDistance(Vector3 point)
    {
        return Vector3.Dot(Normal, point - Point);
    }

    public Plane(Vector3 point, Vector3 normal)
    {
        Normal = Vector3.Normalize(normal);
        Point = point;
    }
}