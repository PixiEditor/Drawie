using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IColorFilterImplementation
{
    public IntPtr CreateBlendMode(Color color, BlendMode blendMode);
    public IntPtr CreateColorMatrix(float[] matrix);
    public IntPtr CreateCompose(ColorFilter outer, ColorFilter inner);
    public void Dispose(ColorFilter colorFilter);
    public object GetNativeColorFilter(IntPtr objectPointer);
    public IntPtr CreateLumaColor();
    public IntPtr CreateHighContrast(bool grayscale, ContrastInvertMode invert, float contrast);
    public IntPtr CreateLighting(Color mul, Color add);
}
