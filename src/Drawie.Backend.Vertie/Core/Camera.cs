using System.Numerics;
using Drawie.Backend.Vertie.Helpers;
using Drawie.Numerics;

namespace Drawie.Backend.Vertie.Core;

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

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, (Position + Forward), Up);

    public Matrix4x4 ProjectionMatrix =>
        Matrix4x4.CreatePerspectiveFieldOfView(MathEx.DegreesToRadians * Zoom, AspectRatio, 0.1f, 100f);

    public Quaternion Rotation => Quaternion.CreateFromYawPitchRoll(-MathEx.DegreesToRadians * Yaw,
        MathEx.DegreesToRadians * Pitch, 0f);

    public Camera(Vector3 position, Vector3 forward, Vector3 up, float aspectRatio)
    {
        Position = position;
        Forward = forward;
        Up = up;
        AspectRatio = aspectRatio;
        SetDirection(0, 0);
        Frustum = new Frustum(this, MathEx.DegreesToRadians * Zoom, 0.1f, 100f);
    }

    public void RecalculateFrustum()
    {
        Frustum = new Frustum(this, MathEx.DegreesToRadians * Zoom, 0.1f, 100f);
    }

    public void SetDirection(float xOffset, float yOffset)
    {
        Yaw += xOffset;
        Pitch -= yOffset;

        Pitch = Math.Clamp(Pitch, -89f, 89f);

        var cameraDirection = Vector3.Zero;
        cameraDirection.X = MathF.Cos(MathEx.DegreesToRadians * Yaw) * MathF.Cos(MathEx.DegreesToRadians * Pitch);
        cameraDirection.Y = MathF.Sin(MathEx.DegreesToRadians * Pitch);
        cameraDirection.Z = MathF.Sin(MathEx.DegreesToRadians * Yaw) * MathF.Cos(MathEx.DegreesToRadians * Pitch);
        cameraDirection = Vector3.Normalize(cameraDirection);

        Forward = cameraDirection;
        Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, cameraDirection));
    }

    public void LookAt(Vector3 target)
    {
        Forward = Vector3.Normalize(target - Position);
        Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Forward));

        Yaw = MathF.Atan2(Forward.Z, Forward.X) * (180f / MathF.PI);
        Pitch = MathF.Asin(Forward.Y) * (180f / MathF.PI);
    }
}
