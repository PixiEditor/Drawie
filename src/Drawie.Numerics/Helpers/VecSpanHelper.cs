using System.Runtime.InteropServices;

namespace Drawie.Numerics.Helpers;

public static class VecSpanHelper
{
    public static Span<VecF> GetSimplestSpanFromEnumerable(IEnumerable<VecF> source)
    {
        if (source is List<VecF> sourceList)
        {
            return CollectionsMarshal.AsSpan(sourceList);
        }

        var sourceArray = source as VecF[] ?? source.ToArray();

        return sourceArray;
    }

    public static Span<VecD> GetSimplestSpanFromEnumerable(IEnumerable<VecD> source)
    {
        if (source is List<VecD> sourceList)
        {
            return CollectionsMarshal.AsSpan(sourceList);
        }

        var sourceArray = source as VecD[] ?? source.ToArray();

        return sourceArray;
    }

    public static Span<float> GetComponentSpan(this Span<VecF> source) => MemoryMarshal.Cast<VecF, float>(source);

    public static Span<double> GetComponentSpan(this Span<VecD> source) => MemoryMarshal.Cast<VecD, double>(source);
}
