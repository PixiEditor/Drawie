using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;

namespace Draiwe.Html5Canvas.Impl;

public class HtmlCanvasSurfaceImplementation : ISurfaceImplementation
{
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
        throw new NotImplementedException();
    }

    public void Flush(DrawingSurface drawingSurface)
    {
        throw new NotImplementedException();
    }

    public DrawingSurface FromNative(object native)
    {
        throw new NotImplementedException();
    }
}