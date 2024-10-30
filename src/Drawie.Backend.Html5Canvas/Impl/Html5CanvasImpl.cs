using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Surfaces.Vector;
using Drawie.Backend.Core.Text;
using Drawie.Html5Canvas.Extensions;
using Drawie.Html5Canvas.Objects;
using Drawie.Numerics;

namespace Drawie.Html5Canvas.Impl;

public class Html5CanvasImpl : HtmlObjectImpl<HtmlCanvasObject>, ICanvasImplementation
{
    public void DrawPixel(IntPtr objPtr, float posX, float posY, Paint drawingPaint)
    {
        throw new NotImplementedException();
    }

    public void DrawSurface(IntPtr objPtr, DrawingSurface drawingSurface, float x, float y, Paint? paint)
    {
        throw new NotImplementedException();
    }

    public void DrawImage(IntPtr objPtr, Image image, float x, float y)
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

    public void DrawPoint(IntPtr objPtr, VecD pos, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawPoints(IntPtr objPtr, PointMode pointMode, VecF[] points, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawRect(IntPtr objPtr, float x, float y, float width, float height, Paint paint)
    {
        HtmlCanvasObject canvasObject = ManagedObjects[(int)objPtr];
        
        SetPaint(canvasObject.Context, paint);

        if (paint.Style == PaintStyle.Fill)
        {
            canvasObject.Context.DrawFillRect(x, y, width, height);
        }
        else if (paint.Style == PaintStyle.Stroke)
        {
            canvasObject.Context.DrawStrokeRect(x, y, width, height);
        }
        else if (paint.Style == PaintStyle.StrokeAndFill)
        {
            canvasObject.Context.DrawFillRect(x, y, width, height);
            canvasObject.Context.DrawStrokeRect(x, y, width, height);
        }
    }

    public void DrawCircle(IntPtr objPtr, float cx, float cy, float radius, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawOval(IntPtr objPtr, float cx, float cy, float width, float height, Paint paint)
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
        HtmlCanvasObject canvasObject = ManagedObjects[(int)objPtr];
        canvasObject.Context.ClearRect(0, 0, canvasObject.Size.X, canvasObject.Size.Y);
    }

    public void Clear(IntPtr objPtr, Color color)
    {
        HtmlCanvasObject canvasObject = ManagedObjects[(int)objPtr];
        
        canvasObject.Context.SetFillStyle(color.ToCssColor());
        canvasObject.Context.DrawFillRect(0, 0, canvasObject.Size.X, canvasObject.Size.Y);
    }

    public void DrawLine(IntPtr objPtr, VecD from, VecD to, Paint paint)
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

    public void DrawBitmap(IntPtr objPtr, Bitmap bitmap, float x, float y)
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

    public void DrawImage(IntPtr objectPointer, Image image, float x, float y, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawRoundRect(IntPtr objectPointer, float x, float y, float width, float height, float radiusX, float radiusY,
        Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawText(IntPtr objectPointer, string text, float x, float y, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawText(IntPtr objectPointer, string text, float x, float y, Font font, Paint paint)
    {
        throw new NotImplementedException();
    }

    public void DrawText(IntPtr objectPointer, string text, float x, float y, TextAlign align, Font font, Paint paint)
    {
        throw new NotImplementedException();
    }

    public int SaveLayer(IntPtr objectPtr)
    {
        throw new NotImplementedException();
    }

    public int SaveLayer(IntPtr objectPtr, Paint paint)
    {
        throw new NotImplementedException();
    }

    public int SaveLayer(IntPtr objectPtr, Paint paint, RectD bounds)
    {
        throw new NotImplementedException();
    }

    public Matrix3X3 GetTotalMatrix(IntPtr objectPointer)
    {
        throw new NotImplementedException();
    }

    public void RotateDegrees(IntPtr objectPointer, float degrees)
    {
        throw new NotImplementedException();
    }

    private void SetPaint(CanvasContext ctx, Paint paint)
    {
        if(paint.Style == PaintStyle.Fill)
        {
            ctx.SetFillStyle(paint.Color.ToCssColor());
        }
        else if (paint.Style == PaintStyle.Stroke)
        {
            ctx.SetStrokeStyle(paint.Color.ToCssColor());
        }
        else if (paint.Style == PaintStyle.StrokeAndFill)
        {
            ctx.SetFillStyle(paint.Color.ToCssColor());
            ctx.SetStrokeStyle(paint.Color.ToCssColor());
        }
    }
}
