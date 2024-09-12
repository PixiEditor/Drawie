using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IImageFilterImplementation
{
    IntPtr CreateMatrixConvolution(VecI size, ReadOnlySpan<float> kernel, float gain, float bias, VecI kernelOffset, TileMode mode, bool convolveAlpha);

    IntPtr CreateCompose(ImageFilter outer, ImageFilter inner);

    object GetNativeImageFilter(IntPtr objPtr);
    
    void DisposeObject(IntPtr objPtr);
}
