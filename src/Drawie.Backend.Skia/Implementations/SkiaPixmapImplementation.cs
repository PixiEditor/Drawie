using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaPixmapImplementation : SkObjectImplementation<SKPixmap>, IPixmapImplementation
    {
        private readonly SkiaColorSpaceImplementation colorSpaceImplementation;

        public SkiaPixmapImplementation(SkiaColorSpaceImplementation colorSpaceImplementation)
        {
            this.colorSpaceImplementation = colorSpaceImplementation;
        }

        public void Dispose(IntPtr objectPointer)
        {
            UnmanageAndDispose(objectPointer);
        }

        public Color GetPixelColor(IntPtr objectPointer, VecI position)
        {
            return this[objectPointer].GetPixelColor(position.X, position.Y).ToBackendColor();
        }

        public ColorF GetPixelColorF(IntPtr objectPointer, VecI position)
        {
            return this[objectPointer].GetPixelColorF(position.X, position.Y).ToBackendColorF();
        }

        public IntPtr GetPixels(IntPtr objectPointer)
        {
            return this[objectPointer].GetPixels();
        }

        public Span<T> GetPixelSpan<T>(Pixmap pixmap)
            where T : unmanaged
        {
            return this[pixmap.ObjectPointer].GetPixelSpan<T>();
        }

        public IntPtr Construct(IntPtr dataPtr, ImageInfo imgInfo)
        {
            SKPixmap pixmap = new SKPixmap(imgInfo.ToSkImageInfo(), dataPtr);
            AddManagedInstance(pixmap);
            return pixmap.Handle;
        }

        public int GetWidth(Pixmap pixmap)
        {
            return this[pixmap.ObjectPointer].Width;
        }

        public int GetHeight(Pixmap pixmap)
        {
            return this[pixmap.ObjectPointer].Height;
        }

        public int GetBytesSize(Pixmap pixmap)
        {
            return this[pixmap.ObjectPointer].BytesSize;
        }

        public object GetNativePixmap(IntPtr objectPointer)
        {
            return this[objectPointer];
        }

        public Pixmap CreateFrom(SKPixmap pixmap)
        {
            AddManagedInstance(pixmap);
            return Pixmap.InternalCreateFromExistingPointer(pixmap.Handle);
        }
    }
}
