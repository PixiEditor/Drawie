using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

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
    public IntPtr Construct(ImageInfo info);
    public bool InstallPixels(IntPtr objectPointer, ImageInfo info, IntPtr pixels);
    public void SetPixels(IntPtr objectPointer, IntPtr pixels);
    public IntPtr GetAddress(IntPtr objectPointer);
}
