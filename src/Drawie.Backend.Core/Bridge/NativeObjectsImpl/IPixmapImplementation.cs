using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IPixmapImplementation
{
    public void Dispose(IntPtr objectPointer);

    public Color GetPixelColor(IntPtr objectPointer, VecI position);

    public ColorF GetPixelColorF(IntPtr objectPointer, VecI position);
    public IntPtr GetPixels(IntPtr objectPointer);

    public Span<T> GetPixelSpan<T>(Pixmap pixmap)
        where T : unmanaged;

    public IntPtr Construct(IntPtr dataPtr, ImageInfo imgInfo);

    public int GetWidth(Pixmap pixmap);

    public int GetHeight(Pixmap pixmap);

    public int GetBytesSize(Pixmap pixmap);
    public object GetNativePixmap(IntPtr objectPointer);
}
