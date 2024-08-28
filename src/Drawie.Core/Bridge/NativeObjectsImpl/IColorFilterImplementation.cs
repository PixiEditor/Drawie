using Drawie.Core.ColorsImpl;
using Drawie.Core.Surfaces;
using Drawie.Core.Surfaces.PaintImpl;

namespace Drawie.Core.Bridge.NativeObjectsImpl;

public interface IColorFilterImplementation
{
    public IntPtr CreateBlendMode(Color color, BlendMode blendMode);
    public IntPtr CreateColorMatrix(float[] matrix);
    public IntPtr CreateCompose(ColorFilter outer, ColorFilter inner);
    public void Dispose(ColorFilter colorFilter);
    public object GetNativeColorFilter(IntPtr objectPointer);
}
