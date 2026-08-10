using System.Numerics;
using Drawie.Numerics;

namespace Drawie.Backend.Vertie;

public class Transform
{
    private Vec3D _position = Vec3D.Zero;
    private float _scale = 1f;
    private Quaternion _rotation = Quaternion.Identity;
    private Matrix4x4 _cachedMatrix = Matrix4x4.Identity;
    
    private bool _isDirty = true;
    
    public Vec3D Position
    {
        get => _position;
        set
        {
            _position = value;
            _isDirty = true;
        }
    }
    
    public Vec3D Right
    {
        get => Vec3D.Transform(Vec3D.UnitX, _rotation);
    }
    
    public Vec3D Up
    {
        get => Vec3D.Transform(Vec3D.UnitY, _rotation);
    }
    
    public Vec3D Forward
    {
        get => Vec3D.Transform(Vec3D.UnitZ, _rotation);
    }

    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            _isDirty = true;
        }
    }
    
    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            _isDirty = true;
        }
    }
    
    public Matrix4x4 ViewMatrix
    {
        get
        {
            if (_isDirty)
            {
                _cachedMatrix = Matrix4x4.Identity * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale) *
                       Matrix4x4.CreateTranslation(Position.ToVector3());
                _isDirty = false;
            }

            return _cachedMatrix;
        }
    }
}