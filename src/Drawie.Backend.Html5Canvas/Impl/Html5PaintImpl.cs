using System.Collections.Concurrent;
using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Surfaces.Vector;
using Drawie.Html5Canvas.Objects;

namespace Drawie.Html5Canvas.Impl;

public class Html5PaintImpl : HtmlObjectImpl<PaintObject>, IPaintImplementation
{
    public IntPtr CreatePaint()
    {
        PaintObject paintObject = new PaintObject();
        ManagedObjects.TryAdd(paintObject.GetHashCode(), paintObject);
        return paintObject.GetHashCode();
    }

    public void Dispose(IntPtr paintObjPointer)
    {
        throw new NotImplementedException();
    }

    public Paint Clone(IntPtr paintObjPointer)
    {
        throw new NotImplementedException();
    }

    public Color GetColor(Paint paint)
    {
        PaintObject paintObject = ManagedObjects[(int)paint.ObjectPointer];
        return paintObject.Color;
    }

    public void SetColor(Paint paint, Color value)
    {
        PaintObject paintObject = ManagedObjects[(int)paint.ObjectPointer];
        paintObject.Color = value;
    }

    public BlendMode GetBlendMode(Paint paint)
    {
        PaintObject paintObject = ManagedObjects[(int)paint.ObjectPointer];
        return paintObject.BlendMode;
    }

    public void SetBlendMode(Paint paint, BlendMode value)
    {
        PaintObject paintObject = ManagedObjects[(int)paint.ObjectPointer];
        paintObject.BlendMode = value;
    }

    public FilterQuality GetFilterQuality(Paint paint)
    {
        PaintObject paintObject = ManagedObjects[(int)paint.ObjectPointer];
        return paintObject.FilterQuality;
    }

    public void SetFilterQuality(Paint paint, FilterQuality value)
    {
        PaintObject paintObject = ManagedObjects[(int)paint.ObjectPointer];
        paintObject.FilterQuality = value;
    }

    public bool GetIsAntiAliased(Paint paint)
    {
        throw new NotImplementedException();
    }

    public void SetIsAntiAliased(Paint paint, bool value)
    {
        throw new NotImplementedException();
    }

    public PaintStyle GetStyle(Paint paint)
    {
        PaintObject paintObject = ManagedObjects[(int)paint.ObjectPointer];
        return paintObject.Style;
    }

    public void SetStyle(Paint paint, PaintStyle value)
    {
        PaintObject paintObject = ManagedObjects[(int)paint.ObjectPointer];
        paintObject.Style = value;
    }

    public StrokeCap GetStrokeCap(Paint paint)
    {
        throw new NotImplementedException();
    }

    public void SetStrokeCap(Paint paint, StrokeCap value)
    {
        throw new NotImplementedException();
    }

    public float GetStrokeWidth(Paint paint)
    {
        throw new NotImplementedException();
    }

    public void SetStrokeWidth(Paint paint, float value)
    {
        throw new NotImplementedException();
    }

    public ColorFilter GetColorFilter(Paint paint)
    {
        throw new NotImplementedException();
    }

    public void SetColorFilter(Paint paint, ColorFilter value)
    {
        throw new NotImplementedException();
    }

    public ImageFilter GetImageFilter(Paint paint)
    {
        throw new NotImplementedException();
    }

    public void SetImageFilter(Paint paint, ImageFilter value)
    {
        throw new NotImplementedException();
    }

    public object GetNativePaint(IntPtr objectPointer)
    {
        throw new NotImplementedException();
    }

    public Shader? GetShader(Paint paint)
    {
        throw new NotImplementedException();
    }

    public void SetShader(Paint paint, Shader shader)
    {
        throw new NotImplementedException();
    }

    public PathEffect GetPathEffect(Paint paint)
    {
        throw new NotImplementedException();
    }

    public void SetPathEffect(Paint paint, PathEffect value)
    {
        throw new NotImplementedException();
    }
}
