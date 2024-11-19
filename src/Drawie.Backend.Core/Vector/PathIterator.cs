using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;

namespace Drawie.Backend.Core.Vector;

public class PathIterator : NativeObject
{
    public PathIterator(IntPtr objPtr) : base(objPtr)
    {
    }

    public override object Native => DrawingBackendApi.Current.PathImplementation.GetNativeIterator(ObjectPointer);

    public bool IsCloseContour => DrawingBackendApi.Current.PathImplementation.IsCloseContour(ObjectPointer);

    public override void Dispose()
    {
        DrawingBackendApi.Current.PathImplementation.DisposeIterator(ObjectPointer);
    }
}
