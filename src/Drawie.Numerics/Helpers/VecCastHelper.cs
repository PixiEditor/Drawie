using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Drawie.Numerics.Helpers;

public static class VecCastHelper
{
    public static VecF[] ToVecFArray(this IEnumerable<VecD> source)
    {
        var sourceSpan = VecSpanHelper.GetSimplestSpanFromEnumerable(source);
        var target = new VecF[sourceSpan.Length];

        var sourceDoubles = sourceSpan.GetComponentSpan();
        var destDoubles = VecSpanHelper.GetComponentSpan(target);

        CastToVecFloatSpan(sourceDoubles, destDoubles);

        return target;
    }

    private static void CastToVecFloatSpan(ReadOnlySpan<double> source, Span<float> target)
    {
        if (Avx.IsSupported && source.Length >= 4)
        {
            var i = 0;
            for (; i < source.Length - 2; i += 4)
            {
                var other = Vector256.Create(source.Slice(i, 4));
                var result = Avx.ConvertToVector128Single(other);

                result.CopyTo(target.Slice(i, 4));
            }

            for (; i < source.Length; i += 2)
            {
                var other = Vector128.Create(source.Slice(i, 2));
                var result = Sse2.ConvertToVector128Single(other);

                result.AsVector2().CopyTo(target.Slice(i, 2));
            }
        }
        else if (Sse2.IsSupported)
        {
            for (var i = 0; i < target.Length; i += 2)
            {
                var other = Vector128.Create(source.Slice(i, 2));
                var result = Sse2.ConvertToVector128Single(other);

                result.AsVector2().CopyTo(target.Slice(i, 2));
            }
        }
        else
        {
            for (var i = 0; i < source.Length; i += 1)
            {
                target[i] = (float)source[i];
            }
        }
    }
}
