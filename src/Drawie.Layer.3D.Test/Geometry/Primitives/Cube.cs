using System.Numerics;
using Drawie.Layer.ThreeD.Test;
using Silk.NET.OpenGL;
using SilkNet.Rendering;

namespace SilkNet.Geometry.Primitives;

public class Cube : GeometryObject
{
    private static readonly float[] Vertices = new[]
    {
        //X    Y      Z       Normals             U     V
        -1f, -1f, -1f,  0.0f,  0.0f, -1.0f, 0.0f, 1.0f,
        1f, -1f, -1f,  0.0f,  0.0f, -1.0f, 1.0f, 1.0f,
        1f,  1f, -1f,  0.0f,  0.0f, -1.0f, 1.0f, 0.0f,
        1f,  1f, -1f,  0.0f,  0.0f, -1.0f, 1.0f, 0.0f,
        -1f,  1f, -1f,  0.0f,  0.0f, -1.0f, 0.0f, 0.0f,
        -1f, -1f, -1f,  0.0f,  0.0f, -1.0f, 0.0f, 1.0f,

        -1f, -1f,  1f,  0.0f,  0.0f,  1.0f, 0.0f, 1.0f,
        1f, -1f,  1f,  0.0f,  0.0f,  1.0f, 1.0f, 1.0f,
        1f,  1f,  1f,  0.0f,  0.0f,  1.0f, 1.0f, 0.0f,
        1f,  1f,  1f,  0.0f,  0.0f,  1.0f, 1.0f, 0.0f,
        -1f,  1f,  1f,  0.0f,  0.0f,  1.0f, 0.0f, 0.0f,
        -1f, -1f,  1f,  0.0f,  0.0f,  1.0f, 0.0f, 1.0f,

        -1f,  1f,  1f, -1.0f,  0.0f,  0.0f, 0.0f, 1.0f,
        -1f,  1f, -1f, -1.0f,  0.0f,  0.0f, 1.0f, 1.0f,
        -1f, -1f, -1f, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
        -1f, -1f, -1f, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
        -1f, -1f,  1f, -1.0f,  0.0f,  0.0f, 0.0f, 0.0f,
        -1f,  1f,  1f, -1.0f,  0.0f,  0.0f, 0.0f, 1.0f,

        1f,  1f,  1f,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f,
        1f,  1f, -1f,  1.0f,  0.0f,  0.0f, 1.0f, 1.0f,
        1f, -1f, -1f,  1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
        1f, -1f, -1f,  1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
        1f, -1f,  1f,  1.0f,  0.0f,  0.0f, 0.0f, 0.0f,
        1f,  1f,  1f,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f,

        -1f, -1f, -1f,  0.0f, -1.0f,  0.0f, 0.0f, 1.0f,
        1f, -1f, -1f,  0.0f, -1.0f,  0.0f, 1.0f, 1.0f,
        1f, -1f,  1f,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f,
        1f, -1f,  1f,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f,
        -1f, -1f,  1f,  0.0f, -1.0f,  0.0f, 0.0f, 0.0f,
        -1f, -1f, -1f,  0.0f, -1.0f,  0.0f, 0.0f, 1.0f,

        -1f,  1f, -1f,  0.0f,  1.0f,  0.0f, 0.0f, 1.0f,
        1f,  1f, -1f,  0.0f,  1.0f,  0.0f, 1.0f, 1.0f,
        1f,  1f,  1f,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f,
        1f,  1f,  1f,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f,
        -1f,  1f,  1f,  0.0f,  1.0f,  0.0f, 0.0f, 0.0f,
        -1f,  1f, -1f,  0.0f,  1.0f,  0.0f, 0.0f, 1.0f
    };

    private static readonly uint[] Indices =
    {
        0, 1, 3,
        1, 2, 3
    };
                
    private BufferObject<float> _vbo;
    
    private BufferObject<uint> _ebo;
    private VertexArrayObject<float, uint> _vao;

    public Cube(GL api, int materialIndex) : base(new GeometryData(Vertices, Indices), materialIndex)
    {
        _ebo = new BufferObject<uint>(api, GeometryData.Indices, BufferTargetARB.ElementArrayBuffer);
        _vbo = new BufferObject<float>(api, GeometryData.Vertices, BufferTargetARB.ArrayBuffer);
        _vao = new VertexArrayObject<float, uint>(api, _vbo, _ebo);
        
        _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 8, 0);
        _vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 8, 3);
        _vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 8, 6);
    }

    public override void OpenDrawingContext()
    {
        _vao.Bind();
    }

    public override void Draw(GL api) 
    {
        api.DrawArrays(PrimitiveType.Triangles, 0, 36);
    }

    public override bool IsInFrustum(Frustum frustum, Transform transform)
    {
        Vector3 position = transform.Position;
        Quaternion rotation = transform.Rotation;

        AABB aabb;

        if (rotation.IsIdentity)
        {
            Vector3 extends = Vector3.One * Transform.Scale;
            Vector3 min = position - extends;
            Vector3 max = position + extends;
            aabb = new AABB(min, max);
        }
        else
        {
            Vector3 right = transform.Right * transform.Scale;
            Vector3 up = transform.Up * transform.Scale;
            Vector3 forward = transform.Forward * transform.Scale;
            
            float x = Math.Abs(Vector3.Dot(Vector3.UnitX, right)) + Math.Abs(Vector3.Dot(Vector3.UnitX, up)) + Math.Abs(Vector3.Dot(Vector3.UnitX, forward));
            float y = Math.Abs(Vector3.Dot(Vector3.UnitY, right)) + Math.Abs(Vector3.Dot(Vector3.UnitY, up)) + Math.Abs(Vector3.Dot(Vector3.UnitY, forward));
            float z = Math.Abs(Vector3.Dot(Vector3.UnitZ, right)) + Math.Abs(Vector3.Dot(Vector3.UnitZ, up)) + Math.Abs(Vector3.Dot(Vector3.UnitZ, forward));
            aabb = new AABB(position, x, y, z);
        }

        
        return aabb.IsOnOrForwardPlane(frustum.Left) && aabb.IsOnOrForwardPlane(frustum.Right) &&
               aabb.IsOnOrForwardPlane(frustum.Top) && aabb.IsOnOrForwardPlane(frustum.Bottom) &&
               aabb.IsOnOrForwardPlane(frustum.Near) && aabb.IsOnOrForwardPlane(frustum.Far);
    }

    public override void Dispose()
    {
        _vbo.Dispose();
        _ebo.Dispose(); 
        _vao.Dispose();
    }
}