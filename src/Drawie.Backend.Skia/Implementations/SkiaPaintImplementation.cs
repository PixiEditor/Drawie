using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaPaintImplementation : SkObjectImplementation<SKPaint>, IPaintImplementation
    {
        private readonly SkiaColorFilterImplementation colorFilterImplementation;
        private readonly SkiaImageFilterImplementation imageFilterImplementation;
        private readonly SkiaShaderImplementation shaderImplementation;
        private readonly SkiaPathEffectImplementation pathEffectImplementation;
        private readonly SkiaBlenderImplementation blenderImplementation;

        public SkiaPaintImplementation(SkiaColorFilterImplementation colorFilterImpl,
            SkiaImageFilterImplementation imageFilterImpl, SkiaShaderImplementation shaderImpl,
            SkiaPathEffectImplementation pathEffectImpl, SkiaBlenderImplementation blenderImpl)
        {
            colorFilterImplementation = colorFilterImpl;
            imageFilterImplementation = imageFilterImpl;
            shaderImplementation = shaderImpl;
            pathEffectImplementation = pathEffectImpl;
            blenderImplementation = blenderImpl;
        }

        public IntPtr CreatePaint()
        {
            SKPaint skPaint = new SKPaint();
            AddManagedInstance(skPaint);
            if (skPaint.ColorFilter != null)
            {
                colorFilterImplementation.AddManagedInstance(skPaint.ColorFilter.Handle, skPaint.ColorFilter);
            }

            return skPaint.Handle;
        }

        public void Dispose(IntPtr paintObjPointer)
        {
            UnmanageAndDispose(paintObjPointer);
        }

        public Paint Clone(IntPtr paintObjPointer)
        {
            SKPaint clone = this[paintObjPointer].Clone();
            AddManagedInstance(clone);

            return new Paint(clone.Handle);
        }

        public Color GetColor(Paint paint)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            return skPaint.Color.ToBackendColor();
        }

        public void SetColor(Paint paint, Color value)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            skPaint.Color = value.ToSKColor();
        }

        public BlendMode GetBlendMode(Paint paint)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            return (BlendMode)skPaint.BlendMode;
        }

        public void SetBlendMode(Paint paint, BlendMode value)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            skPaint.BlendMode = (SKBlendMode)value;
        }

        public FilterQuality GetFilterQuality(Paint paint)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            return (FilterQuality)skPaint.FilterQuality;
        }

        public void SetFilterQuality(Paint paint, FilterQuality value)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            skPaint.FilterQuality = (SKFilterQuality)value;
        }

        public bool GetIsAntiAliased(Paint paint)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            return skPaint.IsAntialias;
        }

        public void SetIsAntiAliased(Paint paint, bool value)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            skPaint.IsAntialias = value;
        }

        public PaintStyle GetStyle(Paint paint)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            return (PaintStyle)skPaint.Style;
        }

        public StrokeJoin GetStrokeJoin(Paint paint)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            return (StrokeJoin)skPaint.StrokeJoin;
        }

        public void SetStrokeJoin(Paint paint, StrokeJoin value)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            skPaint.StrokeJoin = (SKStrokeJoin)value;
        }

        public Blender? GetBlender(Paint paint)
        {
            if (TryGetInstance(paint.ObjectPointer, out var skPaint))
            {
                if (skPaint.Blender == null)
                {
                    return null;
                }

                return new Blender(skPaint.Blender.Handle);
            }

            return null;
        }

        public void SetBlender(Paint paint, Blender? value)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            skPaint.Blender = value != null ? blenderImplementation[value.ObjectPointer] : null;
        }

        public void SetStyle(Paint paint, PaintStyle value)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            skPaint.Style = (SKPaintStyle)value;
        }

        public StrokeCap GetStrokeCap(Paint paint)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            return (StrokeCap)skPaint.StrokeCap;
        }

        public void SetStrokeCap(Paint paint, StrokeCap value)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            skPaint.StrokeCap = (SKStrokeCap)value;
        }

        public float GetStrokeWidth(Paint paint)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            return skPaint.StrokeWidth;
        }

        public void SetStrokeWidth(Paint paint, float value)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            skPaint.StrokeWidth = value;
        }

        public ColorFilter? GetColorFilter(Paint paint)
        {
            if (TryGetInstance(paint.ObjectPointer, out var skPaint))
            {
                if (skPaint.ColorFilter == null)
                {
                    return null;
                }

                return new ColorFilter(skPaint.ColorFilter.Handle);
            }

            return null;
        }

        public void SetColorFilter(Paint paint, ColorFilter? value)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            skPaint.ColorFilter = value == null ? null : colorFilterImplementation[value.ObjectPointer];
        }

        public ImageFilter? GetImageFilter(Paint paint)
        {
            if (TryGetInstance(paint.ObjectPointer, out var skPaint))
            {
                if (skPaint.ImageFilter == null)
                {
                    return null;
                }

                return new ImageFilter(skPaint.ImageFilter.Handle);
            }

            return null;
        }

        public void SetImageFilter(Paint paint, ImageFilter? value)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            skPaint.ImageFilter = value == null ? null : imageFilterImplementation[value.ObjectPointer];
        }

        public Shader? GetShader(Paint paint)
        {
            if (TryGetInstance(paint.ObjectPointer, out var skPaint))
            {
                if (skPaint.Shader == null)
                {
                    return null;
                }

                return new Shader(skPaint.Shader.Handle);
            }

            return null;
        }

        public void SetShader(Paint paint, Shader? shader)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            if (shader == null)
            {
                skPaint.Shader = null;
                return;
            }

            skPaint.Shader = shaderImplementation.GetInstanceOrDefault(shader.ObjectPointer);
        }

        public PathEffect GetPathEffect(Paint paint)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            return new PathEffect(skPaint.PathEffect.Handle);
        }

        public void SetPathEffect(Paint paint, PathEffect? value)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            skPaint.PathEffect = value == null ? null : pathEffectImplementation[value.ObjectPointer];
        }

        public float GetStrokeMiter(Paint paint)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            return skPaint.StrokeMiter;
        }

        public void SetStrokeMiter(Paint paint, float value)
        {
            SKPaint skPaint = this[paint.ObjectPointer];
            skPaint.StrokeMiter = value;
        }

        public object GetNativePaint(IntPtr objectPointer)
        {
            return this[objectPointer];
        }
    }
}
