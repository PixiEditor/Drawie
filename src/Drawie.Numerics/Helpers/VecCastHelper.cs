using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Drawie.Numerics.Helpers;

public static class VecCastHelper
{
    public static VecF[] ToVecFArray(this IEnumerable<VecD> source)
    {
        if (source is List<VecD> sourceList)
        {
            var target = new VecF[sourceList.Count];

            CastToVecFSpan(CollectionsMarshal.AsSpan(sourceList), target);

            return target;
        }
        else
        {
            var sourceArray = source as VecD[] ?? source.ToArray();
            var target = new VecF[sourceArray.Length];

            CastToVecFSpan(sourceArray, target);

            return target;
        }
    }

    public static unsafe void CastToVecFSpan(ReadOnlySpan<VecD> source, Span<VecF> target)
    {
        fixed (VecD* sourcePtr = source)
        {
            fixed (VecF* targetPtr = target)
            {
                // VecD is made of 2 float64 components => source.Length * 2
                var floatSource = new ReadOnlySpan<double>(sourcePtr, source.Length * 2);
                var floatTarget = new Span<float>(targetPtr, target.Length * 2);

                CastToVecFloatSpan(floatSource, (double*)sourcePtr, floatTarget);
            }
        }
    }

    private static unsafe void CastToVecFloatSpan(ReadOnlySpan<double> source, double* sourcePointer, Span<float> target)
    {
        if (Avx.IsSupported && source.Length >= 4)
        {
            var i = 0;
            for (; i < source.Length - 2; i += 4)
            {
                var other = Avx.LoadVector256(sourcePointer + i);
                var result = Avx.ConvertToVector128Single(other);

                result.CopyTo(target.Slice(i, 4));
            }

            for (; i < source.Length; i += 2)
            {
                var other = Sse2.LoadVector128(sourcePointer + i);
                var result = Sse2.ConvertToVector128Single(other);

                result.AsVector2().CopyTo(target.Slice(i, 2));
            }
        }
        else if (Sse2.IsSupported)
        {
            for (var i = 0; i < target.Length; i += 2)
            {
                var other = Sse2.LoadVector128(sourcePointer + i);
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
