using System.Collections.Concurrent;
using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Html5Canvas.Objects;

namespace Drawie.Html5Canvas.Impl;

public class Html5CanvasSurface : ISurfaceImplementation
{
    private int handleCounter = 0;
    private readonly Html5CanvasImpl canvasImpl;
    
    public Html5CanvasSurface(Html5CanvasImpl canvasImpl)
    {
        this.canvasImpl = canvasImpl;
    }
    
    public Pixmap PeekPixels(DrawingSurface drawingSurface)
    {
        throw new NotImplementedException();
    }

    public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixels, int rowBytes)
    {
        throw new NotImplementedException();
    }

    public bool ReadPixels(DrawingSurface drawingSurface, ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX,
        int srcY)
    {
        throw new NotImplementedException();
    }

    public void Draw(DrawingSurface drawingSurface, Canvas surfaceToDraw, int x, int y, Paint drawingPaint)
    {
        throw new NotImplementedException();
    }

    public DrawingSurface Create(ImageInfo imageInfo, IntPtr pixelBuffer)
    {
        throw new NotImplementedException();
    }

    public DrawingSurface Create(Pixmap pixmap)
    {
        throw new NotImplementedException();
    }

    public DrawingSurface Create(ImageInfo imageInfo)
    {
        throw new NotImplementedException();
    }

    public void Dispose(DrawingSurface drawingSurface)
    {
        throw new NotImplementedException();
    }

    public object GetNativeSurface(IntPtr objectPointer)
    {
        if(canvasImpl.ManagedObjects.TryGetValue((int)objectPointer, out HtmlCanvasObject? native))
            return native;
        
        throw new ArgumentException("Object pointer is not valid", nameof(objectPointer));
    }

    public void Flush(DrawingSurface drawingSurface)
    {
        
    }

    public DrawingSurface FromNative(object native)
    {
        if(native is not HtmlCanvasObject canvasObject)
            throw new ArgumentException("Native object is not a HtmlCanvasObject", nameof(native));
        
        return CreateDrawingSurface(canvasObject); 
    }
    
    private DrawingSurface CreateDrawingSurface(HtmlCanvasObject canvasObject)
    {
        canvasImpl.ManagedObjects.TryAdd(canvasObject.Handle, canvasObject);

        handleCounter++;
        
        DrawingSurface drawingSurface = new DrawingSurface(handleCounter, new Canvas(canvasObject.Handle)); 
        return drawingSurface;
    }
}