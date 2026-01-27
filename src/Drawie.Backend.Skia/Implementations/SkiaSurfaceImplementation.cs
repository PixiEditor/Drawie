using System.Diagnostics;
using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaSurfaceImplementation : SkObjectImplementation<SKSurface>, ISurfaceImplementation
    {
        private readonly SkiaPixmapImplementation _pixmapImplementation;
        private readonly SkiaCanvasImplementation _canvasImplementation;
        private readonly SkiaPaintImplementation _paintImplementation;

        internal GRContext? GrContext { get; set; }

        public SkiaSurfaceImplementation(GRContext context, SkiaPixmapImplementation pixmapImplementation,
            SkiaCanvasImplementation canvasImplementation, SkiaPaintImplementation paintImplementation)
        {
            _pixmapImplementation = pixmapImplementation;
            _canvasImplementation = canvasImplementation;
            _paintImplementation = paintImplementation;
            GrContext = context;
        }

        public Pixmap PeekPixels(DrawingSurface drawingSurface)
        {
            SKPixmap pixmap = this[drawingSurface.ObjectPointer].PeekPixels();
            return _pixmapImplementation.CreateFrom(pixmap);
        }

        public bool ReadPixels(DrawingSurface drawingSurface, ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes,
            int srcX,
            int srcY)
        {
            return this[drawingSurface.ObjectPointer]
                .ReadPixels(dstInfo.ToSkImageInfo(), dstPixels, dstRowBytes, srcX, srcY);
        }

        public void Draw(DrawingSurface drawingSurface, Canvas surfaceToDraw, int x, int y, Paint drawingPaint)
        {
            SKCanvas canvas = _canvasImplementation[surfaceToDraw.ObjectPointer];
            SKPaint paint = _paintImplementation[drawingPaint.ObjectPointer];
            var instance = this[drawingSurface.ObjectPointer];
            instance.Draw(canvas, x, y, paint);
        }

        public DrawingSurface? Create(ImageInfo imageInfo, IntPtr pixels, int rowBytes)
        {
            SKSurface? skSurface = CreateSkiaSurface(imageInfo.ToSkImageInfo(), imageInfo.GpuBacked, pixels, rowBytes);
            return CreateDrawingSurface(skSurface);
        }

        public DrawingSurface? Create(ImageInfo imageInfo, IntPtr pixelBuffer)
        {
            SKImageInfo info = imageInfo.ToSkImageInfo();
            SKSurface? skSurface = CreateSkiaSurface(info, imageInfo.GpuBacked, pixelBuffer);
            return CreateDrawingSurface(skSurface);
        }

        private SKSurface? CreateSkiaSurface(SKImageInfo imageInfo, bool isGpuBacked, IntPtr pixels, int rowBytes)
        {
            if (isGpuBacked)
            {
                SKSurface? skSurface = CreateSkiaSurface(imageInfo, true);
                if (skSurface == null)
                {
                    return null;
                }

                using var image = SKImage.FromPixelCopy(imageInfo, pixels, rowBytes);

                var canvas = skSurface.Canvas;
                canvas.DrawImage(image, new SKPoint(0, 0));

                return skSurface;
            }

            return SKSurface.Create(imageInfo, pixels, rowBytes);
        }

        private SKSurface? CreateSkiaSurface(SKImageInfo imageInfo, bool isGpuBacked, IntPtr pixels)
        {
            if (isGpuBacked)
            {
                SKSurface? skSurface = CreateSkiaSurface(imageInfo, true);
                if (skSurface == null)
                {
                    return null;
                }

                using var image = SKImage.FromPixelCopy(imageInfo, pixels);

                var canvas = skSurface.Canvas;
                canvas.DrawImage(image, new SKPoint(0, 0));

                return skSurface;
            }

            return SKSurface.Create(imageInfo, pixels);
        }

        public DrawingSurface? Create(Pixmap pixmap)
        {
            SKPixmap skPixmap = _pixmapImplementation[pixmap.ObjectPointer];
            var skSurface = CreateSkiaSurface(skPixmap);

            return CreateDrawingSurface(skSurface);
        }

        private SKSurface? CreateSkiaSurface(SKPixmap skPixmap)
        {
            SKSurface skSurface = SKSurface.Create(skPixmap);
            return skSurface;
        }

        public DrawingSurface? Create(ImageInfo imageInfo)
        {
            SKSurface? skSurface = CreateSkiaSurface(imageInfo.ToSkImageInfo(), imageInfo.GpuBacked);
            return CreateDrawingSurface(skSurface);
        }

        private SKSurface? CreateSkiaSurface(SKImageInfo info, bool gpu)
        {
            if (!gpu || GrContext == null)
            {
                return SKSurface.Create(info);
            }

            return SKSurface.Create(GrContext, false, info);
        }

        public void Dispose(DrawingSurface drawingSurface)
        {
            UnmanageAndDispose(drawingSurface.ObjectPointer);
        }

        public object GetNativeSurface(IntPtr objectPointer)
        {
            return this[objectPointer];
        }

        private DrawingSurface? CreateDrawingSurface(SKSurface? skSurface)
        {
            if (skSurface == null)
            {
                return null;
            }

#if DRAWIE_TRACE
            Trace(skSurface);
#endif

            _canvasImplementation.AddManagedInstance(skSurface.Canvas.Handle, skSurface.Canvas);
            Canvas canvas = new Canvas(skSurface.Canvas.Handle);

            DrawingSurface surface = new DrawingSurface(skSurface.Handle, canvas);
            AddManagedInstance(skSurface);

            return surface;
        }

        public void Flush(DrawingSurface drawingSurface)
        {
            this[drawingSurface.ObjectPointer].Flush(true, true);
        }

        public DrawingSurface? FromNative(object native)
        {
            if (native is not SKSurface skSurface)
            {
                throw new ArgumentException("Native object is not of type SKSurface");
            }

            return CreateDrawingSurface(skSurface);
        }

        public RectI GetDeviceClipBounds(IntPtr drawingSurface)
        {
            SKRectI skRectI = this[drawingSurface].Canvas.DeviceClipBounds;
            return new RectI(skRectI.Left, skRectI.Top, skRectI.Width, skRectI.Height);
        }

        public void Unmanage(DrawingSurface surface)
        {
            Unmanage(surface.ObjectPointer);
        }

        public RectD GetLocalClipBounds(IntPtr objectPointer)
        {
            SKRect skRect = this[objectPointer].Canvas.LocalClipBounds;
            return new RectD(skRect.Left, skRect.Top, skRect.Width, skRect.Height);
        }
    }
}
