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

        public IntPtr CreateMatrixConvolution(VecI size, ReadOnlySpan<float> kernel, float gain, float bias,
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

            ManagedInstances[skImageFilter.Handle] = skImageFilter;
            return skImageFilter.Handle;
        }

        public IntPtr CreateCompose(ImageFilter outer, ImageFilter inner)
        {
            var skOuter = ManagedInstances[outer.ObjectPointer];
            var skInner = ManagedInstances[inner.ObjectPointer];

            var compose = SKImageFilter.CreateCompose(skOuter, skInner);
            ManagedInstances[compose.Handle] = compose;

            return compose.Handle;
        }

        public object GetNativeImageFilter(IntPtr objPtr) => ManagedInstances[objPtr];

        public IntPtr CreateBlur(float sigmaX, float sigmaY)
        {
            var skImageFilter = SKImageFilter.CreateBlur(sigmaX, sigmaY);
            ManagedInstances[skImageFilter.Handle] = skImageFilter;
            return skImageFilter.Handle;
        }

        public IntPtr CreateDropShadow(float dx, float dy, float sigmaX, float sigmaY, Color color,
            ImageFilter? input)
        {
            SKImageFilter? inputFilter = null;
            if (input != null)
            {
                inputFilter = ManagedInstances[input.ObjectPointer];
            }

            var skImageFilter = SKImageFilter.CreateDropShadow(dx, dy, sigmaX, sigmaY, color.ToSKColor(), inputFilter);
            ManagedInstances[skImageFilter.Handle] = skImageFilter;
            return skImageFilter.Handle;
        }

        public IntPtr CreateShader(Shader shader, bool dither)
        {
            var skShader = ShaderImplementation.ManagedInstances[shader.ObjectPointer];
            var skImageFilter = SKImageFilter.CreateShader(skShader, dither);
            ManagedInstances[skImageFilter.Handle] = skImageFilter;
            return skImageFilter.Handle;
        }

        public IntPtr CreateImage(Image image)
        {
            if (image == null)
            {
                return IntPtr.Zero;
            }


            SKImage target = ImageImplementation.ManagedInstances[image.ObjectPointer];
            var skImageFilter = SKImageFilter.CreateImage(target);
            ManagedInstances[skImageFilter.Handle] = skImageFilter;
            return skImageFilter.Handle;
        }

        public IntPtr CreateTile(RectD source, RectD dest, ImageFilter input)
        {
            if (input == null)
            {
                throw new System.ArgumentNullException(nameof(input));
            }

            var skImageFilter = SKImageFilter.CreateTile(source.ToSKRect(), dest.ToSKRect(),
                ManagedInstances[input.ObjectPointer]);
            ManagedInstances[skImageFilter.Handle] = skImageFilter;
            return skImageFilter.Handle;
        }
    }
}
