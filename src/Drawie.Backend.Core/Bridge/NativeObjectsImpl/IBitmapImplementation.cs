using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using PixiEditor.Numerics;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IBitmapImplementation
{
    public void Dispose(IntPtr objectPointer);
    public Bitmap Decode(ReadOnlySpan<byte> buffer);
    public object GetNativeBitmap(IntPtr objectPointer);
    public Bitmap FromImage(IntPtr snapshot);
    public VecI GetSize(IntPtr objectPointer);
    public byte[] GetBytes(IntPtr objectPointer);
    public ImageInfo GetInfo(IntPtr objectPointer);
    public Pixmap? PeekPixels(IntPtr objectPointer);
}
