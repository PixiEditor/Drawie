using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaImageFilterImplementation : SkObjectImplementation<SKImageFilter>, IImageFilterImplementation
    {
        public SkiaShaderImplementation ShaderImplementation { get; set; }
        public SkiaImageImplementation ImageImplementation { get; set; }

        public IntPtr? CreateMatrixConvolution(VecI size, ReadOnlySpan<float> kernel, float gain, float bias,
            VecI kernelOffset, TileMode mode, bool convolveAlpha)
        {
            var skImageFilter = SKImageFilter.CreateMatrixConvolution(
                new SKSizeI(size.X, size.Y),
                kernel,
                gain,
                bias,
                new SKPointI(kernelOffset.X, kernelOffset.Y),
                (SKShaderTileMode)mode,
                convolveAlpha);

            if (skImageFilter == null)
                return null;

            return AddManagedInstance(skImageFilter);
        }

        public IntPtr? CreateCompose(ImageFilter outer, ImageFilter inner)
        {
            var skOuter = this[outer.ObjectPointer];
            var skInner = this[inner.ObjectPointer];

            var compose = SKImageFilter.CreateCompose(skOuter, skInner);

            if (compose == null)
                return null;

            return AddManagedInstance(compose);
        }

        public IntPtr? CreateBlendMode(BlendMode blendMode, ImageFilter? background, ImageFilter? foreground)
        {
            SKImageFilter? bgFilter = null;
            SKImageFilter? fgFilter = null;

            if (background != null)
            {
                bgFilter = this[background.ObjectPointer];
            }

            if (foreground != null)
            {
                fgFilter = this[foreground.ObjectPointer];
            }

            var skImageFilter = SKImageFilter.CreateBlendMode((SKBlendMode)blendMode, fgFilter, bgFilter);

            if (skImageFilter == null)
                return null;

            return AddManagedInstance(skImageFilter);
        }

        public object GetNativeImageFilter(IntPtr objPtr) => this[objPtr];

        public IntPtr? CreateBlur(float sigmaX, float sigmaY)
        {
            var skImageFilter = SKImageFilter.CreateBlur(sigmaX, sigmaY);
            if (skImageFilter == null)
            {
                return null;
            }

            return AddManagedInstance(skImageFilter);
        }

        public IntPtr? CreateDropShadow(float dx, float dy, float sigmaX, float sigmaY, Color color,
            ImageFilter? input)
        {
            SKImageFilter? inputFilter = null;
            if (input != null)
            {
                inputFilter = this[input.ObjectPointer];
            }

            var skImageFilter = SKImageFilter.CreateDropShadow(dx, dy, sigmaX, sigmaY, color.ToSKColor(), inputFilter);

            if (skImageFilter == null)
                return null;

            return AddManagedInstance(skImageFilter);
        }

        public IntPtr? CreateShader(Shader shader, bool dither)
        {
            var skShader = ShaderImplementation[shader.ObjectPointer];
            var skImageFilter = SKImageFilter.CreateShader(skShader, dither);

            if (skImageFilter == null)
                return null;

            return AddManagedInstance(skImageFilter);
        }

        public IntPtr? CreateImage(Image image)
        {
            if (image == null)
            {
                return IntPtr.Zero;
            }


            SKImage target = ImageImplementation[image.ObjectPointer];
            var skImageFilter = SKImageFilter.CreateImage(target);

            if (skImageFilter == null)
                return null;

            return AddManagedInstance(skImageFilter);
        }

        public IntPtr? CreateTile(RectD source, RectD dest, ImageFilter input)
        {
            if (input == null)
            {
                throw new System.ArgumentNullException(nameof(input));
            }

            var skImageFilter = SKImageFilter.CreateTile(source.ToSKRect(), dest.ToSKRect(),
                this[input.ObjectPointer]);

            if (skImageFilter == null)
                return null;

            return AddManagedInstance(skImageFilter);
        }

        public void DisposeObject(IntPtr objectPointer)
        {
            UnmanageAndDispose(objectPointer);
        }

        public IntPtr? CreateBlendMode(Blender blendMode, ImageFilter? background, ImageFilter? foreground)
        {
            SKImageFilter? bgFilter = null;
            SKImageFilter? fgFilter = null;

            if (background != null)
            {
                bgFilter = this[background.ObjectPointer];
            }

            if (foreground != null)
            {
                fgFilter = this[foreground.ObjectPointer];
            }

            var skBlender = DrawingBackendApi.Current.BlenderImplementation
                .GetNativeObject(blendMode.ObjectPointer) as SKBlender;

            if (skBlender == null)
                return null;

            var skImageFilter = SKImageFilter.CreateBlendMode(skBlender, fgFilter, bgFilter);

            if (skImageFilter == null)
                return null;

            return AddManagedInstance(skImageFilter);
        }

        public IntPtr? CreateBlur(float sigmaX, float sigmaY, TileMode timeMode)
        {
            var skImageFilter = SKImageFilter.CreateBlur(sigmaX, sigmaY, (SKShaderTileMode)timeMode);

            if (skImageFilter == null)
            {
                return null;
            }

            return AddManagedInstance(skImageFilter);
        }

        public IntPtr? CreateDilate(float radiusX, float radiusY)
        {
            var skImageFilter = SKImageFilter.CreateDilate(radiusX, radiusY);

            if (skImageFilter == null)
            {
                return null;
            }

            return AddManagedInstance(skImageFilter);
        }

        public IntPtr? CreateMerge(ImageFilter[] filters)
        {
            SKImageFilter[] skFilters = new SKImageFilter[filters.Length];
            for (int i = 0; i < filters.Length; i++)
            {
                skFilters[i] = this[filters[i].ObjectPointer];
            }

            var skImageFilter = SKImageFilter.CreateMerge(skFilters);

            if (skImageFilter == null)
                return null;

            return AddManagedInstance(skImageFilter);
        }
    }
}
