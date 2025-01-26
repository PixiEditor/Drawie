using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace Drawie.Skia.Implementations
{
    public class SkiaMatrixImplementation : IMatrix3X3Implementation
    {
        public bool TryInvert(Matrix3X3 matrix, out Matrix3X3 inversedResult)
        {
            bool inverted = matrix.ToSkMatrix().TryInvert(out var result);
            inversedResult = result.ToMatrix3X3();

            return inverted;
        }

        public Matrix3X3 Concat(in Matrix3X3 first, in Matrix3X3 second)
        {
            return first.ToSkMatrix().PreConcat(second.ToSkMatrix()).ToMatrix3X3();
        }

        public Matrix3X3 PostConcat(in Matrix3X3 first, in Matrix3X3 second)
        {
            return first.ToSkMatrix().PostConcat(second.ToSkMatrix()).ToMatrix3X3();
        }

        public VecD MapPoint(Matrix3X3 matrix, float p0, float p1)
        {
            var mapped = matrix.ToSkMatrix().MapPoint(p0, p1);
            return new VecD(mapped.X, mapped.Y);
        }

        public VecD MapVector(Matrix3X3 matrix3X3, float x, float y)
        {
            var mapped = matrix3X3.ToSkMatrix().MapVector(x, y);
            return new VecD(mapped.X, mapped.Y);
        }
    }
}
