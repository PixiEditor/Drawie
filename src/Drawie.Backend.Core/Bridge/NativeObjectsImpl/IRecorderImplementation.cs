using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IRecorderImplementation
{
    object GetNativeDrawingRecorder(IntPtr objectPointer);
    void DisposeDrawingRecorder(DrawingRecorder drawingRecorder);
    Canvas BeginRecording(DrawingRecorder drawingRecorder, RectD bounds);
    Picture EndRecordingImmutable(DrawingRecorder recorder);
    IntPtr CreateRecorder();
}
