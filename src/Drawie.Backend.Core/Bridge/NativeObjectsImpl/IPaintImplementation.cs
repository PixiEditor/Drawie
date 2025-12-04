using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl
{
    public interface IPaintImplementation
    {
        public IntPtr CreatePaint();
        public void Dispose(IntPtr paintObjPointer);
        public Paint Clone(IntPtr paintObjPointer);
        public Color GetColor(Paint paint);
        public void SetColor(Paint paint, Color value);
        public BlendMode GetBlendMode(Paint paint);
        public void SetBlendMode(Paint paint, BlendMode value);
        public FilterQuality GetFilterQuality(Paint paint);
        public void SetFilterQuality(Paint paint, FilterQuality value);
        public bool GetIsAntiAliased(Paint paint);
        public void SetIsAntiAliased(Paint paint, bool value);
        public PaintStyle GetStyle(Paint paint);
        public void SetStyle(Paint paint, PaintStyle value);
        public StrokeCap GetStrokeCap(Paint paint);
        public void SetStrokeCap(Paint paint, StrokeCap value);
        public float GetStrokeWidth(Paint paint);
        public void SetStrokeWidth(Paint paint, float value);
        
        public ColorFilter? GetColorFilter(Paint paint);
        public void SetColorFilter(Paint paint, ColorFilter value);
        
        public ImageFilter? GetImageFilter(Paint paint);
        public void SetImageFilter(Paint paint, ImageFilter value);
        
        public object GetNativePaint(IntPtr objectPointer);
        public Shader? GetShader(Paint paint);
        public void SetShader(Paint paint, Shader shader);
        public PathEffect GetPathEffect(Paint paint);
        public void SetPathEffect(Paint paint, PathEffect value);
        public float GetStrokeMiter(Paint paint);
        public void SetStrokeMiter(Paint paint, float value);
        public StrokeJoin GetStrokeJoin(Paint paint);
        public void SetStrokeJoin(Paint paint, StrokeJoin value);
        public Blender? GetBlender(Paint paint);
        public void SetBlender(Paint paint, Blender? value);
    }
}
