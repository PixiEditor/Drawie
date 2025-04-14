using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaBitmapImplementation : SkObjectImplementation<SKBitmap>, IBitmapImplementation
    {
        public SkiaImageImplementation ImageImplementation { get; }

        private readonly SkiaPixmapImplementation _pixmapImplementation;

        public SkiaBitmapImplementation(SkiaImageImplementation imgImpl, SkiaPixmapImplementation pixmapImplementation)
        {
            ImageImplementation = imgImpl;
            _pixmapImplementation = pixmapImplementation;
        }

        public void Dispose(IntPtr objectPointer)
        {
            UnmanageAndDispose(objectPointer);
        }

        public Bitmap Decode(ReadOnlySpan<byte> buffer)
        {
            SKBitmap skBitmap = SKBitmap.Decode(buffer);
            AddManagedInstance(skBitmap);
            return new Bitmap(skBitmap.Handle);
        }

        public Bitmap FromImage(IntPtr ptr)
        {
            SKImage image = ImageImplementation[ptr];
            SKBitmap skBitmap = SKBitmap.FromImage(image);
            AddManagedInstance(skBitmap);
            return new Bitmap(skBitmap.Handle);
        }

        public VecI GetSize(IntPtr objectPointer)
        {
            SKBitmap bitmap = this[objectPointer];
            return new VecI(bitmap.Width, bitmap.Height);
        }

        public byte[] GetBytes(IntPtr objectPointer)
        {
            SKBitmap bitmap = this[objectPointer];
            return bitmap.Bytes;
        }

        public ImageInfo GetInfo(IntPtr objectPointer)
        {
            SKBitmap bitmap = this[objectPointer];
            return bitmap.Info.ToImageInfo();
        }

        public Pixmap PeekPixels(IntPtr objectPointer)
        {
            SKBitmap bitmap = this[objectPointer];
            SKPixmap pixmap = bitmap.PeekPixels();
            return _pixmapImplementation.CreateFrom(pixmap);
        }

        public IntPtr Construct(ImageInfo info)
        {
            SKImageInfo imageInfo = info.ToSkImageInfo();
            SKBitmap bitmap = new SKBitmap(imageInfo);
            AddManagedInstance(bitmap);
            return bitmap.Handle;
        }

        public bool InstallPixels(IntPtr objectPointer, ImageInfo info, IntPtr pixels)
        {
            SKBitmap bitmap = this[objectPointer];
            return bitmap.InstallPixels(info.ToSkImageInfo(), pixels);
        }

        public void SetPixels(IntPtr objectPointer, IntPtr pixels)
        {
            SKBitmap bitmap = this[objectPointer];
            bitmap.SetPixels(pixels);
        }

        public IntPtr GetAddress(IntPtr objectPointer)
        {
            SKBitmap bitmap = this[objectPointer];
            return bitmap.GetAddress(0, 0);
        }

        public object GetNativeBitmap(IntPtr objectPointer)
        {
            return this[objectPointer];
        }
    }
}
