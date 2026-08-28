using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaColorFilterImplementation : SkObjectImplementation<SKColorFilter>, IColorFilterImplementation
    {
        public IntPtr CreateBlendMode(Color color, BlendMode blendMode)
        {
            SKColorFilter skColorFilter = SKColorFilter.CreateBlendMode(color.ToSKColor(), (SKBlendMode)blendMode);
            AddManagedInstance(skColorFilter);

            return skColorFilter.Handle;
        }

        public IntPtr CreateColorMatrix(float[] matrix)
        {
            var skColorFilter = SKColorFilter.CreateColorMatrix(matrix);
            return AddManagedInstance(skColorFilter);
        }

        public IntPtr CreateHighContrast(bool grayscale, ContrastInvertMode invert, float contrast)
        {
            var skColorFilter = SKColorFilter.CreateHighContrast(grayscale, (SKHighContrastConfigInvertStyle)invert, contrast);
            return AddManagedInstance(skColorFilter);
        }

        public IntPtr CreateCompose(ColorFilter outer, ColorFilter inner)
        {
            var skOuter = this[outer.ObjectPointer];
            var skInner = this[inner.ObjectPointer];

            var skColorFilter = SKColorFilter.CreateCompose(skOuter, skInner);
            return AddManagedInstance(skColorFilter);
        }

        public void Dispose(ColorFilter colorFilter)
        {
            UnmanageAndDispose(colorFilter.ObjectPointer);
        }

        public object GetNativeColorFilter(IntPtr objectPointer)
        {
            return this[objectPointer];
        }

        public IntPtr CreateLumaColor()
        {
            var skColorFilter = SKColorFilter.CreateLumaColor();
            return AddManagedInstance(skColorFilter);
        }

        public IntPtr CreateLighting(Color mul, Color add)
        {
            var skColorFilter = SKColorFilter.CreateLighting(mul.ToSKColor(), add.ToSKColor());
            return AddManagedInstance(skColorFilter);
        }

        public IntPtr CreateSrgbToLinearGamma()
        {
            var skColorFilter = SKColorFilter.CreateSrgbToLinearGamma();
            return AddManagedInstance(skColorFilter);
        }

        public IntPtr CreateLinearToSrgbGamma()
        {
            var skColorFilter = SKColorFilter.CreateLinearToSrgbGamma();
            return AddManagedInstance(skColorFilter);
        }
    }
}
