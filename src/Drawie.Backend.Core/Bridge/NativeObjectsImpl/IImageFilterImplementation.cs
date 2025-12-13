using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IImageFilterImplementation
{
    IntPtr CreateMatrixConvolution(VecI size, ReadOnlySpan<float> kernel, float gain, float bias, VecI kernelOffset,
        TileMode mode, bool convolveAlpha);

    IntPtr CreateCompose(ImageFilter outer, ImageFilter inner);

    object GetNativeImageFilter(IntPtr objPtr);

    public IntPtr CreateBlur(float sigmaX, float sigmaY);
    public IntPtr CreateBlendMode(BlendMode blendMode, ImageFilter? background, ImageFilter? foreground);

    public IntPtr CreateDropShadow(float dx, float dy, float sigmaX, float sigmaY, Color color,
        ImageFilter? input);

    public IntPtr CreateShader(Shader shader, bool dither);
    public IntPtr CreateImage(Image image);
    public IntPtr CreateTile(RectD source, RectD dest, ImageFilter input);
    public void DisposeObject(IntPtr objectPointer);
    public IntPtr CreateBlendMode(Blender blendMode, ImageFilter? background, ImageFilter? foreground);
    public IntPtr CreateBlur(float sigmaX, float sigmaY, TileMode timeMode);
    public IntPtr CreateDilate(float radiusX, float radiusY);
    public IntPtr CreateMerge(ImageFilter[] filters);
}
