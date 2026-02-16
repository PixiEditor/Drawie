using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Utils;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Surfaces.PaintImpl
{
    /// <summary>
    ///     Class used to define surface paint, which is a collection of paint operations.
    /// </summary>
    public class Paint : NativeObject
    {
        private ImageFilter? imageFilter;
        private ColorFilter? colorFilter;
        private Shader? shader;

        private Shader? lastShader;
        private Color lastColor;

        public override object Native => DrawingBackendApi.Current.PaintImplementation.GetNativePaint(ObjectPointer);

        public Color Color
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetColor(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetColor(this, value);
        }

        public BlendMode BlendMode
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetBlendMode(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetBlendMode(this, value);
        }

        public float StrokeMiter
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetStrokeMiter(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetStrokeMiter(this, value);
        }

        public StrokeJoin StrokeJoin
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetStrokeJoin(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetStrokeJoin(this, value);
        }

        public FilterQuality FilterQuality
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetFilterQuality(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetFilterQuality(this, value);
        }

        public bool IsAntiAliased
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetIsAntiAliased(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetIsAntiAliased(this, value);
        }

        public PaintStyle Style
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetStyle(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetStyle(this, value);
        }

        public StrokeCap StrokeCap
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetStrokeCap(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetStrokeCap(this, value);
        }

        public float StrokeWidth
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetStrokeWidth(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetStrokeWidth(this, value);
        }

        public ColorFilter ColorFilter
        {
            get => colorFilter ??= DrawingBackendApi.Current.PaintImplementation.GetColorFilter(this);
            set
            {
                DrawingBackendApi.Current.PaintImplementation.SetColorFilter(this, value);
                colorFilter = value;
            }
        }

        public ImageFilter ImageFilter
        {
            get => imageFilter ??= DrawingBackendApi.Current.PaintImplementation.GetImageFilter(this);
            set
            {
                DrawingBackendApi.Current.PaintImplementation.SetImageFilter(this, value);
                imageFilter = value;
            }
        }

        public Shader? Shader
        {
            get => shader ??= DrawingBackendApi.Current.PaintImplementation.GetShader(this);
            set
            {
                DrawingBackendApi.Current.PaintImplementation.SetShader(this, value);
                shader = value;
            }
        }

        public PathEffect? PathEffect
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetPathEffect(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetPathEffect(this, value);
        }

        public Paintable? Paintable { get; set; }

        public Matrix3X3 PaintableMatrix { get; set; } = Matrix3X3.Identity;

        public bool IsOpaqueStandardNonBlendingPaint =>
            Paintable == null &&
            Shader == null &&
            ColorFilter == null &&
            ImageFilter == null &&
            Color.A == 255
            && BlendMode == BlendMode.SrcOver;

        public Blender? Blender
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetBlender(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetBlender(this, value);
        }

        public Paint(IntPtr objPtr) : base(objPtr)
        {
        }

        public Paint() : base(DrawingBackendApi.Current.PaintImplementation.CreatePaint())
        {
        }

        public void SetPaintable(Paintable paintable)
        {
            Paintable = paintable;
        }

        public Paint Clone()
        {
            var paint = DrawingBackendApi.Current.PaintImplementation.Clone(ObjectPointer);
            paint.Paintable = Paintable;

            return paint;
        }

        public override void Dispose()
        {
            DrawingBackendApi.Current.PaintImplementation.Dispose(ObjectPointer);
            if (Paintable != null && Paintable.IsOneTimeUse)
            {
                Paintable.Dispose();
                Paintable = null;
            }
        }

        internal IDisposable ApplyPaintable(RectD bounds, Matrix3X3? matrix = null)
        {
            if (Paintable == null)
            {
                return Disposable.Empty;
            }

            lastShader = Shader;
            lastColor = Color;

            Shaders.Shader? createdShader = null;

            createdShader = Paintable.GetShader(bounds, matrix ?? PaintableMatrix);
            if (createdShader != null)
            {
                Shader = createdShader;
            }

            Paintable.ModifyPaint(this);

            return Disposable.Create(() =>
            {
                Paintable?.DisposeShaderElements();
                createdShader?.Dispose();
                Shader = lastShader;
                Color = lastColor;
            });
        }
    }
}
