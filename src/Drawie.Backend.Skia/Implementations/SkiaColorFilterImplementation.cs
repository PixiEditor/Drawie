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
            AddManagedInstance(skColorFilter);

            return skColorFilter.Handle;
        }

        public IntPtr CreateHighContrast(bool grayscale, ContrastInvertMode invert, float contrast)
        {
            var skColorFilter = SKColorFilter.CreateHighContrast(grayscale, (SKHighContrastConfigInvertStyle)invert, contrast);
            AddManagedInstance(skColorFilter);

            return skColorFilter.Handle;
        }

        public IntPtr CreateCompose(ColorFilter outer, ColorFilter inner)
        {
            var skOuter = this[outer.ObjectPointer];
            var skInner = this[inner.ObjectPointer];

            var skColorFilter = SKColorFilter.CreateCompose(skOuter, skInner);
            AddManagedInstance(skColorFilter);

            return skColorFilter.Handle;
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
            AddManagedInstance(skColorFilter);

            return skColorFilter.Handle;
        }

        public IntPtr CreateLighting(Color mul, Color add)
        {
            var skColorFilter = SKColorFilter.CreateLighting(mul.ToSKColor(), add.ToSKColor());
            AddManagedInstance(skColorFilter);

            return skColorFilter.Handle;
        }
    }
}
