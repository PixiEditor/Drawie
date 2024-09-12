using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Surfaces.Vector;
using PixiEditor.Numerics;

namespace Draiwe.Html5Canvas.Impl;

public class HtmlCanvasImplementation : ICanvasImplementation
{
    public void DrawPixel(IntPtr objPtr, int posX, int posY, Paint drawingPaint)
    {
        throw new NotImplementedException();
    }

    public void DrawSurface(IntPtr objPtr, DrawingSurface drawingSurface, int x, int y, Paint? paint)
    {
        throw new NotImplementedException();
    }

    public void DrawImage(IntPtr objPtr, Image image, int x, int y)
    {
        throw new NotImplementedException();
    }

    public int Save(IntPtr objPtr)
    {
        throw new NotImplementedException();
    }

    public void Restore(IntPtr objPtr)
    {
        throw new NotImplementedException();
    }

    public void Scale(IntPtr objPtr, float sizeX, float sizeY)
    {
        throw new NotImplementedException();
    }

    public void Translate(IntPtr objPtr, float translationX, float translationY)
    {
        throw new NotImplementedException();
    }

    public void DrawPath(IntPtr objPtr, VectorPath path, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawPoint(IntPtr objPtr, VecI pos, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawPoints(IntPtr objPtr, PointMode pointMode, Point[] points, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawRect(IntPtr objPtr, int x, int y, int width, int height, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawCircle(IntPtr objPtr, int cx, int cy, int radius, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawOval(IntPtr objPtr, int cx, int cy, int width, int height, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void ClipPath(IntPtr objPtr, VectorPath clipPath, ClipOperation clipOperation, bool antialias)
    {
        throw new NotImplementedException();
    }

    public void ClipRect(IntPtr objPtr, RectD rect, ClipOperation clipOperation)
    {
        throw new NotImplementedException();
    }

    public void Clear(IntPtr objPtr)
    {
        throw new NotImplementedException();
    }

    public void Clear(IntPtr objPtr, Color color)
    {
        throw new NotImplementedException();
    }

    public void DrawLine(IntPtr objPtr, VecI from, VecI to, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void Flush(IntPtr objPtr)
    {
        throw new NotImplementedException();
    }

    public void SetMatrix(IntPtr objPtr, Matrix3X3 finalMatrix)
    {
        throw new NotImplementedException();
    }

    public void RestoreToCount(IntPtr objPtr, int count)
    {
        throw new NotImplementedException();
    }

    public void DrawColor(IntPtr objPtr, Color color, BlendMode paintBlendMode)
    {
        throw new NotImplementedException();
    }

    public void RotateRadians(IntPtr objPtr, float radians, float centerX, float centerY)
    {
        throw new NotImplementedException();
    }

    public void RotateDegrees(IntPtr objectPointer, float degrees, float centerX, float centerY)
    {
        throw new NotImplementedException();
    }

    public void DrawImage(IntPtr objPtr, Image image, RectD destRect, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawImage(IntPtr objPtr, Image image, RectD sourceRect, RectD destRect, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawBitmap(IntPtr objPtr, Bitmap bitmap, int x, int y)
    {
        throw new NotImplementedException();
    }

    public void Dispose(IntPtr objectPointer)
    {
        throw new NotImplementedException();
    }

    public object GetNativeCanvas(IntPtr objectPointer)
    {
        throw new NotImplementedException();
    }

    public void DrawPaint(IntPtr objectPointer, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawImage(IntPtr objectPointer, Image image, int x, int y, Paint paint)
    {
        throw new NotImplementedException();
    }
}