﻿using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaImageFilterImplementation : SkObjectImplementation<SKImageFilter>, IImageFilterImplementation
    {
        public IntPtr CreateMatrixConvolution(VecI size, ReadOnlySpan<float> kernel, float gain, float bias, VecI kernelOffset, TileMode mode, bool convolveAlpha)
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
    }
}
