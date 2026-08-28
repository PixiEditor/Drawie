using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations;

public class SkiaRecorderImplementation : SkObjectImplementation<SKPictureRecorder>, IRecorderImplementation
{
    private readonly SkiaCanvasImplementation _canvasImpl;
    private readonly SkiaPictureImplementation _pictureImplementation;
    public SkiaRecorderImplementation(SkiaCanvasImplementation canvasImpl,
        SkiaPictureImplementation pictureImplementation)
    {
        _canvasImpl = canvasImpl;
        _pictureImplementation = pictureImplementation;
    }

    public object GetNativeDrawingRecorder(IntPtr objectPointer)
    {
        if(TryGetInstance(objectPointer, out var recorder))
        {
            return recorder ?? throw new InvalidOperationException("SKPictureRecorder instance is null.");
        }
        
        throw new InvalidOperationException("No SKPictureRecorder found for the given pointer.");
    }

    public void DisposeDrawingRecorder(DrawingRecorder drawingRecorder)
    {
        UnmanageAndDispose(drawingRecorder.ObjectPointer);
    }

    public Canvas BeginRecording(DrawingRecorder drawingRecorder, RectD bounds)
    {
        if (TryGetInstance(drawingRecorder.ObjectPointer, out var recorder))
        {
            var skRect = new SKRect((float)bounds.Left, (float)bounds.Top, (float)bounds.Right, (float)bounds.Bottom);
            var canvas = recorder?.BeginRecording(skRect);
            
            if (canvas != null)
            {
                return new Canvas(_canvasImpl.AddManagedInstance(canvas));
            }
            else
            {
                throw new InvalidOperationException("Failed to begin recording on SKPictureRecorder.");
            }
        }
        else
        {
            throw new InvalidOperationException("No SKPictureRecorder found for the given pointer.");
        }
    }

    public Picture EndRecordingImmutable(DrawingRecorder recorder)
    {
        if (TryGetInstance(recorder.ObjectPointer, out var skRecorder))
        {
            var skPicture = skRecorder?.EndRecording();
            if (skPicture != null)
            {
                return new Picture(_pictureImplementation.AddManagedInstance(skPicture));
            }

            throw new InvalidOperationException("Failed to end recording on SKPictureRecorder.");
        }

        throw new InvalidOperationException("No SKPictureRecorder found for the given pointer.");
    }

    public IntPtr CreateRecorder()
    {
        var recorder = new SKPictureRecorder();
        return AddManagedInstance(recorder);
    }
}
