using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Mesh;

public class Vertices : NativeObject
{
    public override object Native => DrawingBackendApi.Current.MeshImplementation.GetNativeVertices(ObjectPointer);
    public IReadOnlyCollection<VecF> Points { get; }

    public Vertices(VertexMode mode, VecF[] points, VecF[] texs, Color[] colors, ushort[] indices)
        : base(DrawingBackendApi.Current.MeshImplementation.Create(mode, points, texs, colors, indices))
    {
        Points = points;
    }

    internal Vertices(IntPtr objPtr) : base(objPtr)
    {
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.MeshImplementation.Dispose(ObjectPointer);
    }
}
