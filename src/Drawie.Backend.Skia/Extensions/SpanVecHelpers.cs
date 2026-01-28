using System.Runtime.InteropServices;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Extensions;

public static class SpanVecHelpers
{
    public static void CopyTo(this Span<VecF> span, Span<SKPoint> target)
    {
        var skPointSpan = MemoryMarshal.Cast<VecF, SKPoint>(span);
        target.CopyTo(skPointSpan);
    }
    
    public static void CopyTo(this Span<SKPoint> span, Span<VecF> target)
    {
        var skPointSpan = MemoryMarshal.Cast<SKPoint, VecF>(span);
        target.CopyTo(skPointSpan);
    }
}
