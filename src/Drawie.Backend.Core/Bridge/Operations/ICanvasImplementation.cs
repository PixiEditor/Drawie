using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Mesh;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.Operations
{
    public interface ICanvasImplementation
    {
        public void DrawPixel(IntPtr objPtr, float posX, float posY, Paint drawingPaint);
        public void DrawSurface(IntPtr objPtr, DrawingSurface drawingSurface, float x, float y, Paint? paint);
        public void DrawImage(IntPtr objPtr, Image image, float x, float y);
        public void DrawImage(IntPtr objPtr, Image image, float x, float y, SamplingOptions samplingOptions);
        public void DrawImage(IntPtr objectPointer, Image image, float x, float y, SamplingOptions samplingOptions, Paint? paint);
        public void DrawImage(IntPtr objectPointer, Image image, RectD sourceRect, RectD destRect, SamplingOptions samplingOptions, Paint? paint);
        public int Save(IntPtr objPtr);
        public void Restore(IntPtr objPtr);
        public void Scale(IntPtr objPtr, float sizeX, float sizeY);
        public void Translate(IntPtr objPtr, float translationX, float translationY);
        public void DrawPath(IntPtr objPtr, VectorPath path, Paint paint);
        public void DrawPoint(IntPtr objPtr, VecD pos, Paint paint);
        public void DrawPoints(IntPtr objPtr, PointMode pointMode, VecF[] points, Paint paint);
        public void DrawRect(IntPtr objPtr, float x, float y, float width, float height, Paint paint);
        public void DrawCircle(IntPtr objPtr, float cx, float cy, float radius, Paint paint);
        public void DrawOval(IntPtr objPtr, float cx, float cy, float width, float height, Paint paint);
        public void ClipPath(IntPtr objPtr, VectorPath clipPath, ClipOperation clipOperation, bool antialias);
        public void ClipRect(IntPtr objPtr, RectD rect, ClipOperation clipOperation);
        public void ClipRoundRect(IntPtr objPtr, RectD rect, VecD radius, ClipOperation clipOperation);
        public void Clear(IntPtr objPtr);
        public void Clear(IntPtr objPtr, Color color);
        public void DrawLine(IntPtr objPtr, VecD from, VecD to, Paint paint);
        public void Flush(IntPtr objPtr);
        public void SetMatrix(IntPtr objPtr, Matrix3X3 finalMatrix);
        public void RestoreToCount(IntPtr objPtr, int count);
        public void DrawColor(IntPtr objPtr, Color color, BlendMode paintBlendMode);
        public void RotateRadians(IntPtr objPtr, float radians, float centerX, float centerY);
        public void RotateDegrees(IntPtr objectPointer, float degrees, float centerX, float centerY);
        public void DrawImage(IntPtr objPtr, Image image, RectD destRect, Paint paint);
        public void DrawImage(IntPtr objPtr, Image image, RectD sourceRect, RectD destRect, Paint paint);
        public void DrawBitmap(IntPtr objPtr, Bitmap bitmap, float x, float y, Paint? paint);
        public void Dispose(IntPtr objectPointer);
        public object GetNativeCanvas(IntPtr objectPointer);
        public void DrawPaint(IntPtr objectPointer, Paint paint);
        public void DrawImage(IntPtr objectPointer, Image image, float x, float y, Paint paint);

        public void DrawRoundRect(IntPtr objectPointer, float x, float y, float width, float height, float radiusX,
            float radiusY, Paint paint);

        public void DrawText(IntPtr objectPointer, string text, float x, float y, Paint paint);
        public void DrawText(IntPtr objectPointer, string text, float x, float y, Font font, Paint paint);

        public void DrawText(IntPtr objectPointer, string text, float x, float y, TextAlign align, Font font,
            Paint paint);

        public int SaveLayer(IntPtr objectPtr);
        public int SaveLayer(IntPtr objectPtr, Paint paint);
        public int SaveLayer(IntPtr objectPtr, Paint paint, RectD bounds);
        public Matrix3X3 GetTotalMatrix(IntPtr objectPointer);
        public void RotateDegrees(IntPtr objectPointer, float degrees);

        public void DrawTextOnPath(IntPtr objectPointer, VectorPath path, string text, float offsetX, float offsetY,
            Font font, Paint paint);

        public RectD GetLocalClipBounds(IntPtr objectPointer);
        public RectI GetDeviceClipBounds(IntPtr objectPointer);
        public void DrawVertices(IntPtr objectPointer, Vertices vertices, BlendMode blendMode, Paint paint);
        public DrawingSurface? GetSurface(Canvas canvas);
    }
}
