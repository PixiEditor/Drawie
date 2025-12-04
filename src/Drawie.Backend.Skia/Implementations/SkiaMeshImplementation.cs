using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Mesh;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations;

public class SkiaMeshImplementation : SkObjectImplementation<SKVertices>, IMeshImplementation
{
    public object GetNativeVertices(IntPtr objectPointer)
    {
        return GetInstanceOrDefault(objectPointer);
    }

    public IntPtr Create(VertexMode mode, VecF[] points, VecF[] texs, Color[] colors, ushort[] indices)
    {
        SKPoint[] skPoints = CastUtility.UnsafeArrayCast<VecF, SKPoint>(points);
        SKPoint[] skTexs = CastUtility.UnsafeArrayCast<VecF, SKPoint>(texs);
        SKColor[] skColors = CastUtility.UnsafeArrayCast<Color, SKColor>(colors);
        var vertices = SKVertices.CreateCopy((SKVertexMode)mode, skPoints, skTexs, skColors, indices);

        AddManagedInstance(vertices);
        return vertices.Handle;
    }

    public void Dispose(IntPtr verticesPointer)
    {
        UnmanageAndDispose(verticesPointer);
    }
}
