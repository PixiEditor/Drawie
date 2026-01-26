using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Mesh;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using Drawie.Skia.Extensions;
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
        private readonly SkObjectImplementation<SKVertices> _verticesImpl;

        public SkiaCanvasImplementation(SkObjectImplementation<SKPaint> paintImpl,
            SkObjectImplementation<SKImage> imageImpl, SkObjectImplementation<SKBitmap> bitmapImpl,
            SkObjectImplementation<SKPath> pathImpl, SkObjectImplementation<SKFont> fontImpl,
            SkObjectImplementation<SKVertices> verticesImpl)
        {
            _paintImpl = paintImpl;
            _imageImpl = imageImpl;
            _bitmapImpl = bitmapImpl;
            _pathImpl = pathImpl;
            _fontImpl = fontImpl;
            _verticesImpl = verticesImpl;
        }

        public void SetSurfaceImplementation(SkiaSurfaceImplementation surfaceImpl)
        {
            _surfaceImpl = surfaceImpl;
        }

        public void DrawPixel(IntPtr objectPointer, float posX, float posY, Paint drawingPaint)
        {
            var canvas = this[objectPointer];
            canvas.DrawPoint(posX, posY, _paintImpl[drawingPaint.ObjectPointer]);
        }

        public void DrawSurface(IntPtr objPtr, DrawingSurface drawingSurface, float x, float y, Paint? paint)
        {
            var canvas = this[objPtr];
            canvas.DrawSurface(
                _surfaceImpl[drawingSurface.ObjectPointer],
                x, y,
                paint != null ? _paintImpl[paint.ObjectPointer] : null);
        }

        public void DrawImage(IntPtr objPtr, Image image, float x, float y)
        {
            var canvas = this[objPtr];
            canvas.DrawImage(_imageImpl[image.ObjectPointer], x, y);
        }

        public void DrawImage(IntPtr objPtr, Image image, float x, float y, SamplingOptions samplingOptions)
        {
            var canvas = this[objPtr];
            canvas.DrawImage(_imageImpl[image.ObjectPointer], x, y, samplingOptions.ToSkSamplingOptions());
        }

        public void DrawImage(IntPtr objPtr, Image image, float x, float y, SamplingOptions samplingOptions,
            Paint? paint)
        {
            var canvas = this[objPtr];
            var skPaint = paint != null ? _paintImpl[paint.ObjectPointer] : null;
            canvas.DrawImage(_imageImpl[image.ObjectPointer], x, y, samplingOptions.ToSkSamplingOptions(), skPaint);
        }

        public void DrawImage(IntPtr objectPointer, Image image, RectD sourceRect, RectD destRect,
            SamplingOptions samplingOptions,
            Paint? paint)
        {
            var canvas = this[objectPointer];
            var skPaint = paint != null ? _paintImpl[paint.ObjectPointer] : null;
            canvas.DrawImage(_imageImpl[image.ObjectPointer], sourceRect.ToSKRect(), destRect.ToSKRect(),
                samplingOptions.ToSkSamplingOptions(), skPaint);
        }

        public void DrawImage(IntPtr objPtr, Image image, float x, float y, Paint paint)
        {
            if (!TryGetInstance(objPtr, out var canvas))
            {
                throw new ObjectDisposedException(nameof(canvas));
            }

            SKPaint? skPaint = null;
            if (paint != null)
            {
                if (!_paintImpl.TryGetInstance(paint.ObjectPointer, out skPaint))
                {
                    throw new ObjectDisposedException(nameof(paint));
                }
            }

            if (!_imageImpl.TryGetInstance(image.ObjectPointer, out var img))
            {
                throw new ObjectDisposedException(nameof(image));
            }

            canvas.DrawImage(img, x, y, skPaint);
        }

        public void DrawRoundRect(IntPtr objectPointer, float x, float y, float width, float height, float radiusX,
            float radiusY,
            Paint paint)
        {
            this[objectPointer]
                .DrawRoundRect(x, y, width, height, radiusX, radiusY, _paintImpl[paint.ObjectPointer]);
        }

        public int Save(IntPtr objPtr)
        {
            return this[objPtr].Save();
        }

        public void Restore(IntPtr objPtr)
        {
            this[objPtr].Restore();
        }

        public void Scale(IntPtr objPtr, float sizeX, float sizeY)
        {
            this[objPtr].Scale(sizeX, sizeY);
        }

        public void Translate(IntPtr objPtr, float translationX, float translationY)
        {
            this[objPtr].Translate(translationX, translationY);
        }

        public void DrawPath(IntPtr objPtr, VectorPath path, Paint paint)
        {
            this[objPtr].DrawPath(
                _pathImpl[path.ObjectPointer],
                _paintImpl[paint.ObjectPointer]);
        }

        public void DrawPoint(IntPtr objPtr, VecD pos, Paint paint)
        {
            this[objPtr].DrawPoint(
                (float)pos.X,
                (float)pos.Y,
                _paintImpl[paint.ObjectPointer]);
        }

        public void DrawPoints(IntPtr objPtr, PointMode pointMode, VecF[] points, Paint paint)
        {
            this[objPtr].DrawPoints(
                (SKPointMode)pointMode,
                CastUtility.UnsafeArrayCast<VecF, SKPoint>(points),
                _paintImpl[paint.ObjectPointer]);
        }

        public void DrawRect(IntPtr objPtr, float x, float y, float width, float height, Paint paint)
        {
            SKPaint skPaint = _paintImpl[paint.ObjectPointer];

            var canvas = this[objPtr];
            canvas.DrawRect(x, y, width, height, skPaint);
        }

        public void DrawCircle(IntPtr objPtr, float cx, float cy, float radius, Paint paint)
        {
            var canvas = this[objPtr];
            canvas.DrawCircle(cx, cy, radius, _paintImpl[paint.ObjectPointer]);
        }

        public void DrawOval(IntPtr objPtr, float cx, float cy, float width, float height, Paint paint)
        {
            var canvas = this[objPtr];
            canvas.DrawOval(cx, cy, width, height, _paintImpl[paint.ObjectPointer]);
        }

        public void ClipPath(IntPtr objPtr, VectorPath clipPath, ClipOperation clipOperation, bool antialias)
        {
            SKCanvas canvas = this[objPtr];
            canvas.ClipPath(_pathImpl[clipPath.ObjectPointer], (SKClipOperation)clipOperation, antialias);
        }

        public void ClipRect(IntPtr objPtr, RectD rect, ClipOperation clipOperation)
        {
            SKCanvas canvas = this[objPtr];
            canvas.ClipRect(rect.ToSKRect(), (SKClipOperation)clipOperation);
        }

        public void ClipRoundRect(IntPtr objPtr, RectD rect, VecD radius, ClipOperation clipOperation)
        {
            SKCanvas canvas = this[objPtr];
            SKRoundRect roundRect = new SKRoundRect(rect.ToSKRect(), (float)radius.X, (float)radius.Y);
            canvas.ClipRoundRect(roundRect, (SKClipOperation)clipOperation);
        }

        public void Clear(IntPtr objPtr)
        {
            this[objPtr].Clear();
        }

        public void Clear(IntPtr objPtr, Color color)
        {
            this[objPtr].Clear(color.ToSKColor());
        }

        public void DrawLine(IntPtr objPtr, VecD from, VecD to, Paint paint)
        {
            var canvas = this[objPtr];
            canvas.DrawLine((float)from.X, (float)from.Y, (float)to.X, (float)to.Y, _paintImpl[paint.ObjectPointer]);
        }

        public void DrawPaint(IntPtr objectPointer, Paint paint)
        {
            var canvas = this[objectPointer];
            canvas.DrawPaint(_paintImpl[paint.ObjectPointer]);
        }

        public void Flush(IntPtr objPtr)
        {
            this[objPtr].Flush();
        }

        public void SetMatrix(IntPtr objPtr, Matrix3X3 finalMatrix)
        {
            SKCanvas canvas = this[objPtr];
            var mappedMatrix = finalMatrix.ToSkMatrix();
            canvas.SetMatrix(in mappedMatrix);
        }

        public void RestoreToCount(IntPtr objPtr, int count)
        {
            this[objPtr].RestoreToCount(count);
        }

        public void DrawColor(IntPtr objPtr, Color color, BlendMode paintBlendMode)
        {
            this[objPtr].DrawColor(color.ToSKColor(), (SKBlendMode)paintBlendMode);
        }

        public void RotateRadians(IntPtr objPtr, float radians, float centerX, float centerY)
        {
            this[objPtr].RotateRadians(radians, centerX, centerY);
        }

        public void RotateDegrees(IntPtr objectPointer, float degrees, float centerX, float centerY)
        {
            this[objectPointer].RotateDegrees(degrees, centerX, centerY);
        }

        public void DrawImage(IntPtr objPtr, Image image, RectD destRect, Paint paint)
        {
            this[objPtr].DrawImage(
                _imageImpl[image.ObjectPointer],
                destRect.ToSKRect(),
                paint == null ? null : _paintImpl[paint.ObjectPointer]);
        }

        public void DrawImage(IntPtr obj, Image image, RectD sourceRect, RectD destRect, Paint paint)
        {
            this[obj].DrawImage(
                _imageImpl[image.ObjectPointer],
                sourceRect.ToSKRect(),
                destRect.ToSKRect(),
                paint == null ? null : _paintImpl[paint.ObjectPointer]);
        }

        public void DrawBitmap(IntPtr objPtr, Bitmap bitmap, float x, float y, Paint? paint)
        {
            this[objPtr].DrawBitmap(_bitmapImpl[bitmap.ObjectPointer], x, y);
        }

        public void DrawText(IntPtr objPtr, string text, float x, float y, Paint paint)
        {
            this[objPtr].DrawText(text, x, y, _paintImpl[paint.ObjectPointer]);
        }
        
        public void DrawText(IntPtr objPtr, string text, float x, float y, Font font, Paint paint)
        {
            SKFont skFont = _fontImpl[font.ObjectPointer];
            this[objPtr].DrawText(text, x, y, skFont, _paintImpl[paint.ObjectPointer]);
        }

        public void DrawText(IntPtr objectPointer, string text, float x, float y, TextAlign align, Font font,
            Paint paint)
        {
            SKFont skFont = _fontImpl[font.ObjectPointer];
            this[objectPointer].DrawText(text, x, y, (SKTextAlign)align, skFont, _paintImpl[paint.ObjectPointer]);
        }

        public int SaveLayer(IntPtr objectPointer)
        {
            return this[objectPointer].SaveLayer();
        }

        public int SaveLayer(IntPtr objectPtr, Paint? paint)
        {
            return this[objectPtr]
                .SaveLayer(paint != null ? _paintImpl[paint.ObjectPointer] : null);
        }

        public int SaveLayer(IntPtr objectPtr, Paint paint, RectD bounds)
        {
            return this[objectPtr]
                .SaveLayer(bounds.ToSKRect(), _paintImpl[paint.ObjectPointer]);
        }

        public Matrix3X3 GetTotalMatrix(IntPtr objectPointer)
        {
            return this[objectPointer].TotalMatrix.ToMatrix3X3();
        }

        public void RotateDegrees(IntPtr objectPointer, float degrees)
        {
            this[objectPointer].RotateDegrees(degrees);
        }

        public void DrawTextOnPath(IntPtr objectPointer, VectorPath path, string text, float offsetX, float offsetY,
            Font font,
            Paint paint)
        {
            this[objectPointer].DrawTextOnPath(
                text,
                _pathImpl[path.ObjectPointer],
                offsetX,
                offsetY,
                _fontImpl[font.ObjectPointer],
                _paintImpl[paint.ObjectPointer]);
        }

        public RectD GetLocalClipBounds(IntPtr objectPointer)
        {
            var clipBounds = this[objectPointer].LocalClipBounds;
            return new RectD(clipBounds.Left, clipBounds.Top, clipBounds.Width, clipBounds.Height);
        }

        public RectI GetDeviceClipBounds(IntPtr objectPointer)
        {
            var clipBounds = this[objectPointer].DeviceClipBounds;
            return new RectI(clipBounds.Left, clipBounds.Top, clipBounds.Width, clipBounds.Height);
        }

        public void DrawVertices(IntPtr objectPointer, Vertices vertices, BlendMode blendMode, Paint paint)
        {
            this[objectPointer].DrawVertices(
                _verticesImpl[vertices.ObjectPointer],
                (SKBlendMode)blendMode,
                _paintImpl[paint.ObjectPointer]);
        }

        public DrawingSurface? GetSurface(Canvas canvas)
        {
            if (TryGetInstance(canvas.ObjectPointer, out var skCanvas))
            {
                var skSurface = skCanvas.Surface;
                if (skSurface != null)
                {
                    var surfaceImpl = _surfaceImpl;
                    if (!surfaceImpl.TryGetInstance(skSurface.Handle, out var surface))
                    {
                        surfaceImpl.AddManagedInstance(skSurface.Handle, skSurface);
                        surface = skSurface;
                    }

                    return surface != null ? new DrawingSurface(surface.Handle, canvas) : null;
                }
            }

            throw new ObjectDisposedException(nameof(canvas));
        }

        public void Dispose(IntPtr objectPointer)
        {
            UnmanageAndDispose(objectPointer);
        }

        public object GetNativeCanvas(IntPtr objectPointer)
        {
            return this[objectPointer];
        }
    }
}
