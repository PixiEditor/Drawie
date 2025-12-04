using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Surfaces;

public class DrawingRecorder : NativeObject
{
    public override object Native =>
        DrawingBackendApi.Current.RecorderImplementation.GetNativeDrawingRecorder(ObjectPointer);

    public DrawingRecorder() : base(DrawingBackendApi.Current.RecorderImplementation.CreateRecorder())
    {
        
    }
    
    internal DrawingRecorder(IntPtr objPtr) : base(objPtr)
    {
    }
    
    public Canvas BeginRecording(RectD bounds)
    {
        return DrawingBackendApi.Current.RecorderImplementation.BeginRecording(this, bounds);
    }
    
    public Picture EndRecordingImmutable()
    {
        return DrawingBackendApi.Current.RecorderImplementation.EndRecordingImmutable(this);
    }
    
    public override void Dispose()
    {
        DrawingBackendApi.Current.RecorderImplementation.DisposeDrawingRecorder(this);
    }
}
