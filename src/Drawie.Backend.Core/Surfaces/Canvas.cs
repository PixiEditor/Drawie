using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Mesh;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Utils;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Surfaces
{
    public delegate void SurfaceChangedEventHandler(RectD? changedRect);

    public class Canvas : NativeObject
    {
        public override object Native => DrawingBackendApi.Current.CanvasImplementation.GetNativeCanvas(ObjectPointer);
        public Matrix3X3 TotalMatrix => DrawingBackendApi.Current.CanvasImplementation.GetTotalMatrix(ObjectPointer);

        public RectD LocalClipBounds =>
            DrawingBackendApi.Current.CanvasImplementation.GetLocalClipBounds(ObjectPointer);

        public RectI DeviceClipBounds =>
            DrawingBackendApi.Current.CanvasImplementation.GetDeviceClipBounds(ObjectPointer);

        public bool IsDisposed { get; private set; }
        public DrawingSurface? Surface => DrawingBackendApi.Current.CanvasImplementation.GetSurface(this);

        public event SurfaceChangedEventHandler? Changed;

        public Canvas(IntPtr objPtr) : base(objPtr)
        {
        }

        public void DrawPixel(VecD position, Paint drawingPaint) =>
            DrawPixel((float)position.X, (float)position.Y, drawingPaint);

        public void DrawPixel(float posX, float posY, Paint drawingPaint)
        {
            RectD rect = new RectD(posX, posY, 1, 1);
            var reset = ApplyPaintable(rect, drawingPaint);
            DrawingBackendApi.Current.CanvasImplementation.DrawPixel(ObjectPointer, posX, posY, drawingPaint);

            reset.Dispose();
            Changed?.Invoke(rect);
        }

        public void DrawSurface(DrawingSurface original, float x, float y, Paint? paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawSurface(ObjectPointer, original, x, y, paint);
            Changed?.Invoke(null);
        }

        public void DrawSurface(DrawingSurface original, float x, float y) => DrawSurface(original, x, y, null);

        public void DrawSurface(DrawingSurface surfaceToDraw, VecD size, Paint paint)
        {
            DrawSurface(surfaceToDraw, (float)size.X, (float)size.Y, paint);
        }

        public void DrawImage(Image image, float x, float y)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawImage(ObjectPointer, image, x, y);
            Changed?.Invoke(new RectD(x, y, image.Width, image.Height));
        }

        public void DrawImage(Image image, float x, float y, SamplingOptions samplingOptions)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawImage(ObjectPointer, image, x, y, samplingOptions);
            Changed?.Invoke(new RectD(x, y, image.Width, image.Height));
        }

        public void DrawImage(Image image, float x, float y, SamplingOptions samplingOptions, Paint? paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawImage(ObjectPointer, image, x, y, samplingOptions,
                paint);
            Changed?.Invoke(new RectD(x, y, image.Width, image.Height));
        }

        public void DrawImage(Image image, float x, float y, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawImage(ObjectPointer, image, x, y, paint);
            Changed?.Invoke(new RectD(x, y, image.Width, image.Height));
        }

        public void DrawImage(Image image, RectD destRect, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawImage(ObjectPointer, image, destRect, paint);
            Changed?.Invoke(destRect);
        }

        public void DrawImage(Image image, RectD sourceRect, RectD destRect, Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawImage(ObjectPointer, image, sourceRect, destRect, paint);
            Changed?.Invoke(destRect);
        }

        public void DrawImage(Image image, RectD sourceRect, RectD destRect, Paint paint, SamplingOptions samplingOptions)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawImage(ObjectPointer, image, sourceRect, destRect,
                samplingOptions, paint);
            Changed?.Invoke(destRect);
        }

        public int Save()
        {
            return DrawingBackendApi.Current.CanvasImplementation.Save(ObjectPointer);
        }

        public void Restore()
        {
            DrawingBackendApi.Current.CanvasImplementation.Restore(ObjectPointer);
        }

        public void Scale(float s) => Scale(s, s);

        /// <param name="size">The amount to scale.</param>
        /// <summary>Pre-concatenates the current matrix with the specified scale.</summary>
        public void Scale(VecF size) => Scale(size.X, size.Y);

        /// <param name="sx">The amount to scale in the x-direction.</param>
        /// <param name="sy">The amount to scale in the y-direction.</param>
        /// <summary>Pre-concatenates the current matrix with the specified scale.</summary>
        public void Scale(float sx, float sy)
        {
            DrawingBackendApi.Current.CanvasImplementation.Scale(ObjectPointer, sx, sy);
        }

        /// <param name="sx">The amount to scale in the x-direction.</param>
        /// <param name="sy">The amount to scale in the y-direction.</param>
        /// <param name="px">The x-coordinate for the scaling center.</param>
        /// <param name="py">The y-coordinate for the scaling center.</param>
        /// <summary>Pre-concatenates the current matrix with the specified scale, at the specific offset.</summary>
        public void Scale(float sx, float sy, float px, float py)
        {
            Translate(px, py);
            Scale(sx, sy);
            Translate(-px, -py);
        }

        public void Translate(VecD vector) => Translate((float)vector.X, (float)vector.Y);

        public void Translate(float translationX, float translationY)
        {
            DrawingBackendApi.Current.CanvasImplementation.Translate(ObjectPointer, translationX, translationY);
        }

        public void DrawPath(VectorPath path, Paint paint)
        {
            var reset = ApplyPaintable(path.TightBounds, paint);
            DrawingBackendApi.Current.CanvasImplementation.DrawPath(ObjectPointer, path, paint);
            reset.Dispose();
            Changed?.Invoke(path.Bounds);
        }

        // Draw Point and Draw Points do not work correctly on GPU surfaces. Seems like a Skia issue.

        /*public void DrawPoint(VecD pos, Paint paint)
        {
            RectD rect = new RectD(pos.X, pos.Y, 1, 1);
            var reset = ApplyPaintable(rect, paint);
            DrawingBackendApi.Current.CanvasImplementation.DrawPoint(ObjectPointer, pos, paint);
            reset.Dispose();
            Changed?.Invoke(rect);
        }

        public void DrawPoints(PointMode pointMode, VecF[] points, Paint paint)
        {
            RectD? rect = RectD.FromPoints(points);
            var reset = ApplyPaintable(rect, paint);
            DrawingBackendApi.Current.CanvasImplementation.DrawPoints(ObjectPointer, pointMode, points, paint);
            reset.Dispose();
            Changed?.Invoke(RectD.FromPoints(points));
        }

        public void DrawPoints(PointMode pointMode, VecF[] points, Paint paint, RectD paintableBounds)
        {
            var reset = ApplyPaintable(paintableBounds, paint);
            DrawingBackendApi.Current.CanvasImplementation.DrawPoints(ObjectPointer, pointMode, points, paint);
            reset.Dispose();
            Changed?.Invoke(RectD.FromPoints(points));
        }*/

        public void DrawRect(float x, float y, float width, float height, Paint paint)
        {
            RectD rect = new RectD(x, y, width, height);
            var reset = ApplyPaintable(rect, paint);

            DrawingBackendApi.Current.CanvasImplementation.DrawRect(ObjectPointer, x, y, width, height, paint);
            reset.Dispose();
            Changed?.Invoke(rect);
        }

        public void DrawCircle(float centerX, float centerY, float radius, Paint paint)
        {
            RectD rect = new RectD(centerX - radius, centerY - radius, radius * 2, radius * 2);
            var reset = ApplyPaintable(rect, paint);
            DrawingBackendApi.Current.CanvasImplementation.DrawCircle(ObjectPointer, centerX, centerY, radius, paint);
            reset.Dispose();
            Changed?.Invoke(rect);
        }

        public void DrawCircle(VecD center, float radius, Paint paint) =>
            DrawCircle((float)center.X, (float)center.Y, radius, paint);

        public void DrawOval(float centerX, float centerY, float radiusX, float radiusY, Paint paint)
        {
            RectD rect = new RectD(centerX - radiusX, centerY - radiusY, radiusX * 2, radiusY * 2);
            var reset = ApplyPaintable(rect, paint);
            DrawingBackendApi.Current.CanvasImplementation.DrawOval(ObjectPointer, centerX, centerY, radiusX, radiusY,
                paint);

            reset.Dispose();
            Changed?.Invoke(rect);
        }

        public void DrawOval(VecD center, VecD radius, Paint paint) =>
            DrawOval((float)center.X, (float)center.Y, (float)radius.X, (float)radius.Y, paint);

        public void DrawRect(RectI rect, Paint paint) => DrawRect(rect.X, rect.Y, rect.Width, rect.Height, paint);

        public void DrawRect(RectD rect, Paint paint) =>
            DrawRect((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, paint);

        public void DrawRoundRect(float x, float y, float width, float height, float radiusX, float radiusY,
            Paint paint)
        {
            RectD rect = new RectD(x, y, width, height);
            var reset = ApplyPaintable(rect, paint);
            DrawingBackendApi.Current.CanvasImplementation.DrawRoundRect(ObjectPointer, x, y, width, height, radiusX,
                radiusY, paint);
            reset.Dispose();
            Changed?.Invoke(rect);
        }

        public void DrawText(string text, VecD position, Paint paint)
        {
            using Font defaultFont = Font.CreateDefault();
            defaultFont.MeasureText(text, out RectD bounds, paint);

            bounds = new RectD(position.X + bounds.X, position.Y + bounds.Y, bounds.Width, bounds.Height);
            var reset = ApplyPaintable(bounds, paint);
            DrawingBackendApi.Current.CanvasImplementation.DrawText(ObjectPointer, text, (float)position.X,
                (float)position.Y, paint);

            reset.Dispose();
            Changed?.Invoke(bounds);
        }

        public void DrawText(string text, VecD position, Font font, Paint paint)
        {
            font.MeasureText(text, out RectD bounds, paint);
            bounds = new RectD(position.X + bounds.X, position.Y + bounds.Y, bounds.Width, bounds.Height);
            var reset = ApplyPaintable(bounds, paint);
            DrawingBackendApi.Current.CanvasImplementation.DrawText(ObjectPointer, text, (float)position.X,
                (float)position.Y, font, paint);

            reset.Dispose();
            Changed?.Invoke(bounds);
        }

        public void DrawText(string text, VecD position, TextAlign align, Font font, Paint paint)
        {
            font.MeasureText(text, out RectD bounds, paint);
            bounds = new RectD(position.X + bounds.X, position.Y + bounds.Y, bounds.Width, bounds.Height);
            var reset = ApplyPaintable(bounds, paint);
            DrawingBackendApi.Current.CanvasImplementation.DrawText(ObjectPointer, text, (float)position.X,
                (float)position.Y, align, font, paint);

            reset.Dispose();

            Changed?.Invoke(bounds);
        }

        public void ClipPath(VectorPath clipPath) => ClipPath(clipPath, ClipOperation.Intersect);

        public void ClipPath(VectorPath clipPath, ClipOperation clipOperation) =>
            ClipPath(clipPath, clipOperation, false);

        public void ClipPath(VectorPath clipPath, ClipOperation clipOperation, bool antialias)
        {
            DrawingBackendApi.Current.CanvasImplementation.ClipPath(ObjectPointer, clipPath, clipOperation, antialias);
        }

        public void ClipRect(RectD rect, ClipOperation clipOperation = ClipOperation.Intersect)
        {
            DrawingBackendApi.Current.CanvasImplementation.ClipRect(ObjectPointer, rect, clipOperation);
        }

        public void ClipRoundRect(RectD rect, VecD radius, ClipOperation clipOperation)
        {
            DrawingBackendApi.Current.CanvasImplementation.ClipRoundRect(ObjectPointer, rect, radius, clipOperation);
        }

        public void Clear()
        {
            DrawingBackendApi.Current.CanvasImplementation.Clear(ObjectPointer);
            Changed?.Invoke(null);
        }

        public void Clear(Color color)
        {
            DrawingBackendApi.Current.CanvasImplementation.Clear(ObjectPointer, color);
            Changed?.Invoke(null);
        }

        public void DrawLine(VecD from, VecD to, Paint paint)
        {
            ShapeCorners corners = new ShapeCorners(from, to, paint.StrokeWidth);
            IDisposable? reset = null;
            if (paint?.Paintable != null)
            {
                if (paint.Paintable.AbsoluteValues)
                {
                    reset = paint.ApplyPaintable(LocalClipBounds, Matrix3X3.Identity);
                }
                else
                {
                    if (paint.Paintable is IStartEndPaintable startEndPaintable)
                    {
                        var start = startEndPaintable.Start;
                        var end = startEndPaintable.End;
                        bool absolute = paint.Paintable.AbsoluteValues;

                        paint.Paintable.AbsoluteValues = true;
                        startEndPaintable.Start = from;
                        startEndPaintable.End = to;

                        reset = paint.ApplyPaintable(corners.AABBBounds, Matrix3X3.Identity);

                        paint.Paintable.AbsoluteValues = absolute;
                        startEndPaintable.Start = start;
                        startEndPaintable.End = end;
                    }
                    else
                    {
                        Matrix3X3 rotationMatrix = Matrix3X3.CreateRotation((float)corners.RectRotation,
                            (float)corners.RectCenter.X, (float)corners.RectCenter.Y);
                        var unrotated = corners.AsRotated(-corners.RectRotation, corners.RectCenter);
                        reset = paint.ApplyPaintable(unrotated.AABBBounds, rotationMatrix);
                    }
                }
            }

            DrawingBackendApi.Current.CanvasImplementation.DrawLine(ObjectPointer, from, to, paint);
            reset?.Dispose();
            Changed?.Invoke(corners.AABBBounds);
        }

        public void Flush()
        {
            DrawingBackendApi.Current.CanvasImplementation.Flush(ObjectPointer);
        }

        public void SetMatrix(Matrix3X3 finalMatrix)
        {
            DrawingBackendApi.Current.CanvasImplementation.SetMatrix(ObjectPointer, finalMatrix);
        }

        public void RestoreToCount(int count)
        {
            DrawingBackendApi.Current.CanvasImplementation.RestoreToCount(ObjectPointer, count);
        }

        public void DrawColor(Color color, BlendMode paintBlendMode)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawColor(ObjectPointer, color, paintBlendMode);
            Changed?.Invoke(null);
        }

        public void DrawPaintable(Paintable paintable, BlendMode blendMode)
        {
            var shader = paintable.GetShader(LocalClipBounds, Matrix3X3.Identity);
            if (shader != null)
            {
                using Paint paint = new Paint() { Shader = shader, BlendMode = blendMode };
                DrawPaint(paint);
                shader.Dispose();
            }
            else
            {
                using Paint paint = new Paint() { BlendMode = blendMode };
                paintable.ModifyPaint(paint);
                DrawPaint(paint);
            }
        }

        public void DrawPaintable(Paintable paintable, BlendMode blendMode, RectD rect)
        {
            if (paintable is ColorPaintable colorPaintable)
            {
                DrawColor(colorPaintable.Color, blendMode);
            }
            else
            {
                using Paint paint = new Paint() { Paintable = paintable, BlendMode = blendMode };
                IDisposable reset = ApplyPaintable(rect, paint);
                DrawPaint(paint);

                reset.Dispose();
            }
        }

        public void RotateRadians(float dataAngle, float centerX, float centerY)
        {
            DrawingBackendApi.Current.CanvasImplementation.RotateRadians(ObjectPointer, dataAngle, centerX, centerY);
        }

        public void RotateDegrees(float degrees, float centerX, float centerY)
        {
            DrawingBackendApi.Current.CanvasImplementation.RotateDegrees(ObjectPointer, degrees, centerX, centerY);
        }

        public void DrawBitmap(Bitmap bitmap, int x, int y, Paint? paint = null)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawBitmap(ObjectPointer, bitmap, x, y, paint);
            Changed?.Invoke(null);
        }

        public void DrawPaint(Paint paint)
        {
            DrawingBackendApi.Current.CanvasImplementation.DrawPaint(ObjectPointer, paint);
            Changed?.Invoke(null);
        }

        public override void Dispose()
        {
            IsDisposed = true;
            DrawingBackendApi.Current.CanvasImplementation.Dispose(ObjectPointer);
        }

        public int SaveLayer()
        {
            return DrawingBackendApi.Current.CanvasImplementation.SaveLayer(ObjectPointer);
        }

        public int SaveLayer(Paint paint)
        {
            return DrawingBackendApi.Current.CanvasImplementation.SaveLayer(ObjectPointer, paint);
        }

        public int SaveLayer(Paint paint, RectD bounds)
        {
            return DrawingBackendApi.Current.CanvasImplementation.SaveLayer(ObjectPointer, paint, bounds);
        }

        public void RotateDegrees(float degrees)
        {
            DrawingBackendApi.Current.CanvasImplementation.RotateDegrees(ObjectPointer, degrees);
        }

        public void DrawTextOnPath(VectorPath path, string text, VecD offset, Font font, Paint paint)
        {
            // below is not very precise
            RectD bounds = path.Bounds.Inflate(font.Size);
            bounds = bounds.Offset(offset);
            var reset = ApplyPaintable(bounds, paint);
            DrawingBackendApi.Current.CanvasImplementation.DrawTextOnPath(ObjectPointer, path, text, (float)offset.X,
                (float)offset.Y, font, paint);

            reset.Dispose();
            Changed?.Invoke(bounds);
        }

        private IDisposable ApplyPaintable(RectD? rect, Paint paint, Matrix3X3? matrix = null)
        {
            if (paint?.Paintable != null)
            {
                if (paint.Paintable.AbsoluteValues || rect == null)
                {
                    return paint.ApplyPaintable(LocalClipBounds, matrix);
                }

                return paint.ApplyPaintable(rect.Value, matrix);
            }

            return Disposable.Empty;
        }

        public void DrawVertices(Vertices vertices, BlendMode blendMode, Paint paint)
        {
            RectD? rect = RectD.FromPoints(vertices.Points.ToArray());
            var reset = ApplyPaintable(rect, paint);
            DrawingBackendApi.Current.CanvasImplementation.DrawVertices(ObjectPointer, vertices, blendMode, paint);
            reset.Dispose();
            Changed?.Invoke(rect);
        }
    }
}
