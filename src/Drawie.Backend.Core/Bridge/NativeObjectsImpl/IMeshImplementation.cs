using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Mesh;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IMeshImplementation
{
    public IntPtr Create(VertexMode mode, VecF[] points, VecF[] texs, Color[] colors, ushort[] indices);
    public object GetNativeVertices(IntPtr objectPointer);
    public void Dispose(IntPtr verticesPointer);
}
