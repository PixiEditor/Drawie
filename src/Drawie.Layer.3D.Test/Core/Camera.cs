using System.Numerics;
using SilkNet.Rendering;

namespace SilkNet;

public class Camera
{
    public Vector3 Position { get; set; }
    public Vector3 Forward { get; private set; }
    public Vector3 Up { get; private set; }
    public Vector3 Right { get; private set; }
    public float AspectRatio { get; set; }

    public float Yaw { get; set; } = -90f;
    public float Pitch { get; set; }
    public Frustum Frustum { get; private set; }

    private float _zoom = 45f;
    public float Zoom
    {
        get => _zoom;
        set => _zoom = Math.Clamp(Zoom - value, 1f, 45f);
    }

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Position + Forward, Up);

    public Matrix4x4 ProjectionMatrix =>
        Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Zoom), AspectRatio, 0.1f, 100f);

    public Quaternion Rotation => Quaternion.CreateFromYawPitchRoll(-MathHelper.DegreesToRadians(Yaw),
        MathHelper.DegreesToRadians(Pitch), 0f);

    public Camera(Vector3 position, Vector3 forward, Vector3 up, float aspectRatio)
    {
        Position = position;
        Forward = forward;
        Up = up;
        AspectRatio = aspectRatio;
        SetDirection(0, 0);
        Frustum = new Frustum(this, MathHelper.DegreesToRadians(Zoom), 0.1f, 100f);
    }

    public void RecalculateFrustum()
    {
        Frustum = new Frustum(this, MathHelper.DegreesToRadians(Zoom), 0.1f, 100f);
    }

    public void SetDirection(float xOffset, float yOffset)
    {
        Yaw += xOffset;
        Pitch -= yOffset;

        Pitch = Math.Clamp(Pitch, -89f, 89f);

        var cameraDirection = Vector3.Zero;
        cameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
        cameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
        cameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch));
        cameraDirection = Vector3.Normalize(cameraDirection);

        Forward = cameraDirection;
        Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, cameraDirection));
    }
}