using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IMatrix3X3Implementation
{
    public bool TryInvert(Matrix3X3 matrix, out Matrix3X3 inversedResult);
    public Matrix3X3 Concat(in Matrix3X3 first, in Matrix3X3 second);
    public Matrix3X3 PostConcat(in Matrix3X3 first, in Matrix3X3 second);
    public VecD MapPoint(Matrix3X3 matrix, float p0, float p1);
    public VecD MapVector(Matrix3X3 matrix3X3, float x, float y);
}
