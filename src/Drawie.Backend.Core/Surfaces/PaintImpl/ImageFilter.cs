using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Surfaces.PaintImpl;

public class ImageFilter : NativeObject
{
    public ImageFilterType WellKnownType { get; init; }

    public ImageFilter(IntPtr objPtr) : base(objPtr)
    {
    }

    public static ImageFilter? CreateMatrixConvolution(VecI size, ReadOnlySpan<float> kernel, float gain, float bias,
        VecI kernelOffset,
        TileMode tileMode, bool convolveAlpha)
    {
        var filter = DrawingBackendApi.Current.ImageFilterImplementation.CreateMatrixConvolution(
            size,
            kernel,
            gain,
            bias,
            kernelOffset,
            tileMode,
            convolveAlpha);

        if (filter == null)
            return null;

        return new ImageFilter(filter.Value) { WellKnownType = ImageFilterType.MatrixConvolution };
    }

    public static ImageFilter? CreateMatrixConvolution(Kernel kernel, float gain, float bias, VecI kernelOffset,
        TileMode tileMode, bool convolveAlpha) =>
        CreateMatrixConvolution(new VecI(kernel.Width, kernel.Height), kernel.AsSpan(), gain, bias, kernelOffset,
            tileMode, convolveAlpha);

    public static ImageFilter? CreateMatrixConvolution(KernelArray kernel, float gain, float bias, VecI kernelOffset,
        TileMode tileMode, bool convolveAlpha) =>
        CreateMatrixConvolution(new VecI(kernel.Width, kernel.Height), kernel.AsSpan(), gain, bias, kernelOffset,
            tileMode, convolveAlpha);

    /// <param name="outer">The outer (second) filter to apply.</param>
    /// <param name="inner">The inner (first) filter to apply.</param>
    /// <summary>Creates an image filter, whose effect is to first apply the inner filter and then apply the outer filter to the result of the inner.</summary>
    /// <returns>Returns the new <see cref="T:Drawie.Backend.Core.Surface.PaintImpl.ImageFilter" />, or null on error.</returns>
    public static ImageFilter? CreateCompose(ImageFilter outer, ImageFilter inner)
    {
        var filter = DrawingBackendApi.Current.ImageFilterImplementation.CreateCompose(outer, inner);
        if (filter == null)
            return null;

        return new ImageFilter(filter.Value) { WellKnownType = ImageFilterType.Compose };
    }

    public static ImageFilter? CreateBlendMode(BlendMode mode, ImageFilter? background, ImageFilter? foreground)
    {
        var filter = DrawingBackendApi.Current.ImageFilterImplementation.CreateBlendMode(mode, background, foreground);
        if (filter == null)
            return null;

        return new ImageFilter(filter.Value) { WellKnownType = ImageFilterType.BlendMode };
    }

    public override object Native =>
        DrawingBackendApi.Current.ImageFilterImplementation.GetNativeImageFilter(ObjectPointer);


    public override void Dispose()
    {
        DrawingBackendApi.Current.ImageFilterImplementation.DisposeObject(ObjectPointer);
    }

    public static ImageFilter? CreateBlur(float sigmaX, float sigmaY)
    {
        var blur = DrawingBackendApi.Current.ImageFilterImplementation.CreateBlur(sigmaX, sigmaY);
        if (blur == null)
        {
            return null;
        }

        return new ImageFilter(blur.Value) { WellKnownType = ImageFilterType.Blur };
    }

    public static ImageFilter? CreateDropShadow(float dx, float dy, float sigmaX, float sigmaY, Color color,
        ImageFilter? input)
    {
        var shadow = DrawingBackendApi.Current.ImageFilterImplementation.CreateDropShadow(dx, dy, sigmaX,
            sigmaY, color, input);

        if (shadow == null)
            return null;

        return new ImageFilter(shadow.Value) { WellKnownType = ImageFilterType.DropShadow };
    }

    public static ImageFilter? CreateShader(Shader? shader, bool dither)
    {
        var filter = DrawingBackendApi.Current.ImageFilterImplementation.CreateShader(shader, dither);

        if (filter == null)
            return null;

        return new ImageFilter(filter.Value) { WellKnownType = ImageFilterType.Shader };
    }

    public static ImageFilter? CreateImage(Image image)
    {
        var filter = DrawingBackendApi.Current.ImageFilterImplementation.CreateImage(image);

        if (filter == null)
            return null;

        return new ImageFilter(filter.Value)
        {
            WellKnownType = ImageFilterType.Image
        };
    }

    public static ImageFilter? CreateTile(RectD source, RectD destination, ImageFilter input)
    {
        var filter =
            DrawingBackendApi.Current.ImageFilterImplementation.CreateTile(source, destination, input);

        if (filter == null)
            return null;

        return new ImageFilter(filter.Value) { WellKnownType = ImageFilterType.Tile };
    }

    public static ImageFilter? CreateBlendMode(Blender blender, ImageFilter? background, ImageFilter? foreground)
    {
        var filter = DrawingBackendApi.Current.ImageFilterImplementation.CreateBlendMode(blender, background,
            foreground);

        if (filter == null)
            return null;

        return new ImageFilter(filter.Value) { WellKnownType = ImageFilterType.BlendMode };
    }

    public static ImageFilter? CreateBlur(float sigmaX, float sigmaY, TileMode repeat)
    {
        var filter = DrawingBackendApi.Current.ImageFilterImplementation.CreateBlur(sigmaX, sigmaY, repeat);

        if (filter == null)
            return null;

        return new ImageFilter(filter.Value) { WellKnownType = ImageFilterType.Blur };
    }

    public static ImageFilter? CreateDilate(float radiusX, float radiusY)
    {
        var dilate = DrawingBackendApi.Current.ImageFilterImplementation.CreateDilate(radiusX, radiusY);

        if (dilate == null)
            return null;

        return new ImageFilter(dilate.Value) { WellKnownType = ImageFilterType.Dilate };
    }

    public static ImageFilter? CreateMerge(params ImageFilter[] filters)
    {
        var filter = DrawingBackendApi.Current.ImageFilterImplementation.CreateMerge(filters);

        if (filter == null)
            return null;

        return new ImageFilter(filter.Value) { WellKnownType = ImageFilterType.Merge };
    }
}

public enum ImageFilterType
{
    Unknown,
    MatrixConvolution,
    Compose,
    BlendMode,
    Blur,
    DropShadow,
    Shader,
    Image,
    Tile,
    Dilate,
    Merge
}
