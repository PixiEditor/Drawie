using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Drawie.Numerics.Helpers;

public static class VecMinMaxHelper
{
    public static (float minX, float maxX, float minY, float maxY) GetMinMax(this IEnumerable<VecF> source)
    {
        var span = VecSpanHelper.GetSimplestSpanFromEnumerable(source);

        if (span == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return MinMaxFromVecFloatSpan(span.GetComponentSpan());
    }

    private static (float minX, float maxX, float minY, float maxY) MinMaxFromVecFloatSpan(
        ReadOnlySpan<float> floatSpan)
    {
        if (floatSpan.Length > 20)
        {
            if (Vector512.IsHardwareAccelerated)
            {
                const int byteSize = 512 / 8 / sizeof(float);
                int i = 0;

                var minVec = Vector512.Create(float.MaxValue);
                var maxVec = Vector512.Create(float.MinValue);

                ref float start = ref MemoryMarshal.GetReference(floatSpan);

                for (; i <= floatSpan.Length - byteSize; i += byteSize)
                {
                    var xVec = Vector512.LoadUnsafe(ref start, (nuint)i);
                    minVec = Vector512.Min(minVec, xVec);
                    maxVec = Vector512.Max(maxVec, xVec);
                }

                var minX = minVec[0];
                var maxX = maxVec[0];
                var minY = minVec[1];
                var maxY = maxVec[1];
                for (var j = 2; j < byteSize; j += 2)
                {
                    minX = Math.Min(minX, minVec[j]);
                    maxX = Math.Max(maxX, maxVec[j]);
                    minY = Math.Min(minY, minVec[j + 1]);
                    maxY = Math.Max(maxY, maxVec[j + 1]);
                }

                for (; i < floatSpan.Length; i += 2)
                {
                    float x = floatSpan[i];
                    float y = floatSpan[i + 1];
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }

                return (minX, maxX, minY, maxY);
            }

            if (Vector256.IsHardwareAccelerated)
            {
                const int byteSize = 256 / 8 / sizeof(float);
                int i = 0;

                var minVec = Vector256.Create(float.MaxValue);
                var maxVec = Vector256.Create(float.MinValue);

                ref float start = ref MemoryMarshal.GetReference(floatSpan);

                for (; i <= floatSpan.Length - byteSize; i += byteSize)
                {
                    var xVec = Vector256.LoadUnsafe(ref start, (nuint)i);
                    minVec = Vector256.Min(minVec, xVec);
                    maxVec = Vector256.Max(maxVec, xVec);
                }

                var minX = minVec[0];
                var maxX = maxVec[0];
                var minY = minVec[1];
                var maxY = maxVec[1];
                for (var j = 2; j < byteSize; j += 2)
                {
                    minX = Math.Min(minX, minVec[j]);
                    maxX = Math.Max(maxX, maxVec[j]);
                    minY = Math.Min(minY, minVec[j + 1]);
                    maxY = Math.Max(maxY, maxVec[j + 1]);
                }

                for (; i < floatSpan.Length; i += 2)
                {
                    float x = floatSpan[i];
                    float y = floatSpan[i + 1];
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }

                return (minX, maxX, minY, maxY);
            }
        }

        {
            var minY = float.MaxValue;
            var minX = float.MaxValue;
            var maxY = float.MinValue;
            var maxX = float.MinValue;

            for (var i = 0; i < floatSpan.Length; i += 2)
            {
                var x = floatSpan[i];
                var y = floatSpan[i + 1];

                if (x < minX)
                    minX = x;
                if (x > maxX)
                    maxX = x;

                if (y < minY)
                    minY = y;
                if (y > maxY)
                    maxY = y;
            }

            return (minX, maxX, minY, maxY);
        }
    }
}
