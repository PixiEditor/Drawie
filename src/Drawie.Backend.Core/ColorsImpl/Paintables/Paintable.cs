using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public abstract class Paintable : IDisposable, ICloneable
{
    public abstract bool AnythingVisible { get; }
    public bool AbsoluteValues { get; set; } = false;
    public bool IsOneTimeUse { get; set; } = false;
    public RectD? Bounds { get; set; }

    public abstract Shader? GetShader(RectD bounds, Matrix3X3 matrix);

    public static implicit operator Paintable(Color color) => new ColorPaintable(color);
    public abstract Paintable? Clone();
    object ICloneable.Clone() => Clone();
    public abstract void ApplyOpacity(double opacity);
    public virtual void Dispose() { }
    public virtual void DisposeShaderElements() { }

    public virtual void ModifyPaint(Paint paint) { }
}
