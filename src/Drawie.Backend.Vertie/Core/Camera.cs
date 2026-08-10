using System.Numerics;
using Drawie.Numerics;

namespace Drawie.Backend.Vertie;

public class Camera
{
    public Vec3D Position { get; set; }
    public Vec3D Forward { get; private set; }
    public Vec3D Up { get; private set; }
    public Vec3D Right { get; private set; }
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

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position.ToVector3(), (Position + Forward).ToVector3(), Up.ToVector3());

    public Matrix4x4 ProjectionMatrix =>
        Matrix4x4.CreatePerspectiveFieldOfView(MathEx.DegreesToRadians * Zoom, AspectRatio, 0.1f, 100f);

    public Quaternion Rotation => Quaternion.CreateFromYawPitchRoll(-MathEx.DegreesToRadians * Yaw,
        MathEx.DegreesToRadians * Pitch, 0f);

    public Camera(Vec3D position, Vec3D forward, Vec3D up, float aspectRatio)
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

        var cameraDirection = Vec3D.Zero;
        cameraDirection.X = MathF.Cos(MathEx.DegreesToRadians * Yaw) * MathF.Cos(MathEx.DegreesToRadians * Pitch);
        cameraDirection.Y = MathF.Sin(MathEx.DegreesToRadians * Pitch);
        cameraDirection.Z = MathF.Sin(MathEx.DegreesToRadians * Yaw) * MathF.Cos(MathEx.DegreesToRadians * Pitch);
        cameraDirection = cameraDirection.Normalize();

        Forward = cameraDirection;
        Right = Forward.Cross(Vec3D.UnitY).Normalize();
        Up = Right.Cross(cameraDirection).Normalize();
    }
}