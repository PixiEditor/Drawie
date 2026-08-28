using System.Numerics;

namespace SilkNet.Rendering;

public struct Frustum
{
    public Plane Near { get; private set; }
    public Plane Far { get; private set; }
    public Plane Left { get; private set; }
    public Plane Right { get; private set; }
    public Plane Top { get; private set; }
    public Plane Bottom { get; private set; }

    public Frustum(Camera camera, float fovY, float zNear, float zFar) : this()
    {
        Recalculate(camera, fovY, zNear, zFar);
    }

    public void Recalculate(Camera camera, float fovY, float zNear, float zFar)
    {
        float halfVerticalSize = zFar * (float)Math.Tan(fovY / 2);
        float halfHorizontalSize = halfVerticalSize * camera.AspectRatio;
        Vector3 frontMultFar = zFar * camera.Forward;
        
        Near = new Plane(camera.Position + zNear * camera.Forward, camera.Forward);
        Far = new Plane(camera.Position + frontMultFar, -camera.Forward);
        Right = new Plane(camera.Position, Vector3.Cross(camera.Up, frontMultFar + camera.Right * halfHorizontalSize));
        Left = new Plane(camera.Position, Vector3.Cross(frontMultFar - camera.Right * halfHorizontalSize, camera.Up));
        Bottom = new Plane(camera.Position, Vector3.Cross(camera.Right, frontMultFar - camera.Up * halfVerticalSize));
        Top = new Plane(camera.Position, Vector3.Cross(frontMultFar + camera.Up * halfVerticalSize, camera.Right));
    }
}