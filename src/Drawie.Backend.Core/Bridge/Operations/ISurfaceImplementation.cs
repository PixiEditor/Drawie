using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.Operations;

public interface ISurfaceImplementation
{
    public Pixmap PeekPixels(DrawingSurface drawingSurface);
    public DrawingSurface? Create(ImageInfo imageInfo, IntPtr pixels, int rowBytes);
    public bool ReadPixels(DrawingSurface drawingSurface, ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY);
    public void Draw(DrawingSurface drawingSurface, Canvas surfaceToDraw, int x, int y, Paint drawingPaint);
    public DrawingSurface? Create(ImageInfo imageInfo, IntPtr pixelBuffer);
    public DrawingSurface? Create(Pixmap pixmap);
    public DrawingSurface? Create(ImageInfo imageInfo);
    public void Dispose(DrawingSurface drawingSurface);
    public object GetNativeSurface(IntPtr objectPointer);
    public void Flush(DrawingSurface drawingSurface);
    public DrawingSurface? FromNative(object native);
    public RectI GetDeviceClipBounds(IntPtr drawingSurface);
    public void Unmanage(DrawingSurface surface);
    public RectD GetLocalClipBounds(IntPtr objectPointer);
}

