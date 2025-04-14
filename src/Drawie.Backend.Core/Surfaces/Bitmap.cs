using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Surfaces;

public class Bitmap : NativeObject
{
    public VecI Size
    {
        get => DrawingBackendApi.Current.BitmapImplementation.GetSize(ObjectPointer);
    }

    public Bitmap(IntPtr objPtr) : base(objPtr)
    {
    }

    public Bitmap(ImageInfo info) : base(DrawingBackendApi.Current.BitmapImplementation.Construct(info))
    {
    }

    public override object Native => DrawingBackendApi.Current.BitmapImplementation.GetNativeBitmap(ObjectPointer);
    public byte[] Bytes => DrawingBackendApi.Current.BitmapImplementation.GetBytes(ObjectPointer);
    public ImageInfo Info => DrawingBackendApi.Current.BitmapImplementation.GetInfo(ObjectPointer);
    public IntPtr Address => DrawingBackendApi.Current.BitmapImplementation.GetAddress(ObjectPointer);

    public override void Dispose()
    {
        DrawingBackendApi.Current.BitmapImplementation.Dispose(ObjectPointer);
    }

    public static Bitmap Decode(ReadOnlySpan<byte> buffer)
    {
        return DrawingBackendApi.Current.BitmapImplementation.Decode(buffer);
    }

    public static Bitmap FromImage(Image snapshot)
    {
        return DrawingBackendApi.Current.BitmapImplementation.FromImage(snapshot.ObjectPointer);
    }

    public Pixmap? PeekPixels()
    {
        return DrawingBackendApi.Current.BitmapImplementation.PeekPixels(ObjectPointer);
    }

    public bool InstallPixels(ImageInfo info, IntPtr pixels)
    {
        return DrawingBackendApi.Current.BitmapImplementation.InstallPixels(ObjectPointer, info, pixels);
    }

    public void SetPixels(IntPtr pixels)
    {
        DrawingBackendApi.Current.BitmapImplementation.SetPixels(ObjectPointer, pixels);
    }
}
