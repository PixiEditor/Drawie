using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public sealed class SkiaCanvasImplementation : SkObjectImplementation<SKCanvas>, ICanvasImplementation
    {
        private readonly SkObjectImplementation<SKPaint> _paintImpl;
        private SkObjectImplementation<SKSurface> _surfaceImpl;
        private readonly SkObjectImplementation<SKImage> _imageImpl;
        private readonly SkObjectImplementation<SKBitmap> _bitmapImpl;
        private readonly SkObjectImplementation<SKPath> _pathImpl;
        private readonly SkObjectImplementation<SKFont> _fontImpl;

        public SkiaCanvasImplementation(SkObjectImplementation<SKPaint> paintImpl,
            SkObjectImplementation<SKImage> imageImpl, SkObjectImplementation<SKBitmap> bitmapImpl,
            SkObjectImplementation<SKPath> pathImpl, SkObjectImplementation<SKFont> fontImpl)
        {
            _paintImpl = paintImpl;
            _imageImpl = imageImpl;
            _bitmapImpl = bitmapImpl;
            _pathImpl = pathImpl;
            _fontImpl = fontImpl;
        }

        public void SetSurfaceImplementation(SkiaSurfaceImplementation surfaceImpl)
        {
            _surfaceImpl = surfaceImpl;
        }

        public void DrawPixel(IntPtr objectPointer, float posX, float posY, Paint drawingPaint)
        {
            var canvas = ManagedInstances[objectPointer];
            canvas.DrawPoint(posX, posY, _paintImpl.ManagedInstances[drawingPaint.ObjectPointer]);
        }

        public void DrawSurface(IntPtr objPtr, DrawingSurface drawingSurface, float x, float y, Paint? paint)
        {
            var canvas = ManagedInstances[objPtr];
            canvas.DrawSurface(
                _surfaceImpl.ManagedInstances[drawingSurface.ObjectPointer],
                x, y,
                paint != null ? _paintImpl.ManagedInstances[paint.ObjectPointer] : null);
        }

        public void DrawImage(IntPtr objPtr, Image image, float x, float y)
        {
            var canvas = ManagedInstances[objPtr];
            canvas.DrawImage(_imageImpl.ManagedInstances[image.ObjectPointer], x, y);
        }

        public void DrawImage(IntPtr objPtr, Image image, float x, float y, Paint paint)
        {
            if (!ManagedInstances.TryGetValue(objPtr, out var canvas))
            {
                throw new ObjectDisposedException(nameof(canvas));
            }

            SKPaint? skPaint = null;
            if (paint != null)
            {
                if (!_paintImpl.ManagedInstances.TryGetValue(paint.ObjectPointer, out skPaint))
                {
                    throw new ObjectDisposedException(nameof(paint));
                }
            }

            if (!_imageImpl.ManagedInstances.TryGetValue(image.ObjectPointer, out var img))
            {
                throw new ObjectDisposedException(nameof(image));
            }

            canvas.DrawImage(img, x, y, skPaint);
        }

        public void DrawRoundRect(IntPtr objectPointer, float x, float y, float width, float height, float radiusX,
            float radiusY,
            Paint paint)
        {
            ManagedInstances[objectPointer]
                .DrawRoundRect(x, y, width, height, radiusX, radiusY, _paintImpl[paint.ObjectPointer]);
        }

        public int Save(IntPtr objPtr)
        {
            return ManagedInstances[objPtr].Save();
        }

        public void Restore(IntPtr objPtr)
        {
            ManagedInstances[objPtr].Restore();
        }

        public void Scale(IntPtr objPtr, float sizeX, float sizeY)
        {
            ManagedInstances[objPtr].Scale(sizeX, sizeY);
        }

        public void Translate(IntPtr objPtr, float translationX, float translationY)
        {
            ManagedInstances[objPtr].Translate(translationX, translationY);
        }

        public void DrawPath(IntPtr objPtr, VectorPath path, Paint paint)
        {
            ManagedInstances[objPtr].DrawPath(
                _pathImpl[path.ObjectPointer],
                _paintImpl[paint.ObjectPointer]);
        }

        public void DrawPoint(IntPtr objPtr, VecD pos, Paint paint)
        {
            ManagedInstances[objPtr].DrawPoint(
                (float)pos.X,
                (float)pos.Y,
                _paintImpl[paint.ObjectPointer]);
        }

        public void DrawPoints(IntPtr objPtr, PointMode pointMode, VecF[] points, Paint paint)
        {
            ManagedInstances[objPtr].DrawPoints(
                (SKPointMode)pointMode,
                CastUtility.UnsafeArrayCast<VecF, SKPoint>(points),
                _paintImpl[paint.ObjectPointer]);
        }

        public void DrawRect(IntPtr objPtr, float x, float y, float width, float height, Paint paint)
        {
            SKPaint skPaint = _paintImpl[paint.ObjectPointer];

            var canvas = ManagedInstances[objPtr];
            canvas.DrawRect(x, y, width, height, skPaint);
        }

        public void DrawCircle(IntPtr objPtr, float cx, float cy, float radius, Paint paint)
        {
            var canvas = ManagedInstances[objPtr];
            canvas.DrawCircle(cx, cy, radius, _paintImpl[paint.ObjectPointer]);
        }

        public void DrawOval(IntPtr objPtr, float cx, float cy, float width, float height, Paint paint)
        {
            var canvas = ManagedInstances[objPtr];
            canvas.DrawOval(cx, cy, width, height, _paintImpl[paint.ObjectPointer]);
        }

        public void ClipPath(IntPtr objPtr, VectorPath clipPath, ClipOperation clipOperation, bool antialias)
        {
            SKCanvas canvas = ManagedInstances[objPtr];
            canvas.ClipPath(_pathImpl[clipPath.ObjectPointer], (SKClipOperation)clipOperation, antialias);
        }

        public void ClipRect(IntPtr objPtr, RectD rect, ClipOperation clipOperation)
        {
            SKCanvas canvas = ManagedInstances[objPtr];
            canvas.ClipRect(rect.ToSKRect(), (SKClipOperation)clipOperation);
        }

        public void ClipRoundRect(IntPtr objPtr, RectD rect, VecD radius, ClipOperation clipOperation)
        {
            SKCanvas canvas = ManagedInstances[objPtr];
            SKRoundRect roundRect = new SKRoundRect(rect.ToSKRect(), (float)radius.X, (float)radius.Y);
            canvas.ClipRoundRect(roundRect, (SKClipOperation)clipOperation);
        }

        public void Clear(IntPtr objPtr)
        {
            ManagedInstances[objPtr].Clear();
        }

        public void Clear(IntPtr objPtr, Color color)
        {
            ManagedInstances[objPtr].Clear(color.ToSKColor());
        }

        public void DrawLine(IntPtr objPtr, VecD from, VecD to, Paint paint)
        {
            var canvas = ManagedInstances[objPtr];
            canvas.DrawLine((float)from.X, (float)from.Y, (float)to.X, (float)to.Y, _paintImpl[paint.ObjectPointer]);
        }

        public void DrawPaint(IntPtr objectPointer, Paint paint)
        {
            var canvas = ManagedInstances[objectPointer];
            canvas.DrawPaint(_paintImpl[paint.ObjectPointer]);
        }

        public void Flush(IntPtr objPtr)
        {
            ManagedInstances[objPtr].Flush();
        }

        public void SetMatrix(IntPtr objPtr, Matrix3X3 finalMatrix)
        {
            SKCanvas canvas = ManagedInstances[objPtr];
            canvas.SetMatrix(finalMatrix.ToSkMatrix());
        }

        public void RestoreToCount(IntPtr objPtr, int count)
        {
            ManagedInstances[objPtr].RestoreToCount(count);
        }

        public void DrawColor(IntPtr objPtr, Color color, BlendMode paintBlendMode)
        {
            ManagedInstances[objPtr].DrawColor(color.ToSKColor(), (SKBlendMode)paintBlendMode);
        }

        public void RotateRadians(IntPtr objPtr, float radians, float centerX, float centerY)
        {
            ManagedInstances[objPtr].RotateRadians(radians, centerX, centerY);
        }

        public void RotateDegrees(IntPtr objectPointer, float degrees, float centerX, float centerY)
        {
            ManagedInstances[objectPointer].RotateDegrees(degrees, centerX, centerY);
        }

        public void DrawImage(IntPtr objPtr, Image image, RectD destRect, Paint paint)
        {
            ManagedInstances[objPtr].DrawImage(
                _imageImpl[image.ObjectPointer],
                destRect.ToSKRect(),
                paint == null ? null : _paintImpl[paint.ObjectPointer]);
        }

        public void DrawImage(IntPtr obj, Image image, RectD sourceRect, RectD destRect, Paint paint)
        {
            ManagedInstances[obj].DrawImage(
                _imageImpl[image.ObjectPointer],
                sourceRect.ToSKRect(),
                destRect.ToSKRect(),
                paint == null ? null : _paintImpl[paint.ObjectPointer]);
        }

        public void DrawBitmap(IntPtr objPtr, Bitmap bitmap, float x, float y)
        {
            ManagedInstances[objPtr].DrawBitmap(_bitmapImpl[bitmap.ObjectPointer], x, y);
        }

        public void DrawText(IntPtr objPtr, string text, float x, float y, Paint paint)
        {
            ManagedInstances[objPtr].DrawText(text, x, y, _paintImpl[paint.ObjectPointer]);
        }
        
        public void DrawText(IntPtr objPtr, string text, float x, float y, Font font, Paint paint)
        {
            SKFont skFont = _fontImpl[font.ObjectPointer];
            ManagedInstances[objPtr].DrawText(text, x, y, skFont, _paintImpl[paint.ObjectPointer]);
        }

        public void DrawText(IntPtr objectPointer, string text, float x, float y, TextAlign align, Font font, Paint paint)
        {
            SKFont skFont = _fontImpl[font.ObjectPointer];
            ManagedInstances[objectPointer].DrawText(text, x, y, (SKTextAlign)align, skFont, _paintImpl[paint.ObjectPointer]);
        }

        public int SaveLayer(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer].SaveLayer();
        }

        public int SaveLayer(IntPtr objectPtr, Paint? paint)
        {
            return ManagedInstances[objectPtr]
                .SaveLayer(paint != null ? _paintImpl.ManagedInstances[paint.ObjectPointer] : null);
        }

        public int SaveLayer(IntPtr objectPtr, Paint paint, RectD bounds)
        {
            return ManagedInstances[objectPtr]
                .SaveLayer(bounds.ToSKRect(), _paintImpl.ManagedInstances[paint.ObjectPointer]);
        }

        public Matrix3X3 GetTotalMatrix(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer].TotalMatrix.ToMatrix3X3();
        }

        public void RotateDegrees(IntPtr objectPointer, float degrees)
        {
            ManagedInstances[objectPointer].RotateDegrees(degrees);
        }

        public void DrawTextOnPath(IntPtr objectPointer, VectorPath path, string text, float offsetX, float offsetY, Font font,
            Paint paint)
        {
            ManagedInstances[objectPointer].DrawTextOnPath(
                text,
                _pathImpl[path.ObjectPointer],
                offsetX,
                offsetY,
                _fontImpl[font.ObjectPointer],
                _paintImpl[paint.ObjectPointer]);
        }

        public RectD GetLocalClipBounds(IntPtr objectPointer)
        {
            var clipBounds = ManagedInstances[objectPointer].LocalClipBounds;
            return new RectD(clipBounds.Left, clipBounds.Top, clipBounds.Width, clipBounds.Height);
        }

        public RectI GetDeviceClipBounds(IntPtr objectPointer)
        {
            var clipBounds = ManagedInstances[objectPointer].DeviceClipBounds;
            return new RectI(clipBounds.Left, clipBounds.Top, clipBounds.Width, clipBounds.Height);
        }

        public void Dispose(IntPtr objectPointer)
        {
            ManagedInstances[objectPointer].Dispose();

            ManagedInstances.TryRemove(objectPointer, out _);
        }

        public object GetNativeCanvas(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer];
        }
    }
}
