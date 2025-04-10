using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaColorSpaceImplementation : SkObjectImplementation<SKColorSpace>, IColorSpaceImplementation
    {
        private readonly IntPtr _srgbPointer;
        private readonly IntPtr _srgbLinearPointer;

        private Dictionary<IntPtr, SKColorSpaceTransferFn> _transferFunctions = new();
        private int functionsCount = 0;

        public SkiaColorSpaceImplementation()
        {
            _srgbPointer = SKColorSpace.CreateSrgb().Handle;
            _srgbLinearPointer = SKColorSpace.CreateSrgbLinear().Handle;
        }

        public ColorSpace CreateSrgb()
        {
            SKColorSpace skColorSpace = SKColorSpace.CreateSrgb();
            AddManagedInstance(skColorSpace);
            return new ColorSpace(skColorSpace.Handle);
        }

        public ColorSpace CreateSrgbLinear()
        {
            SKColorSpace skColorSpace = SKColorSpace.CreateSrgbLinear();
            AddManagedInstance(skColorSpace);
            return new ColorSpace(skColorSpace.Handle);
        }

        public void Dispose(IntPtr objectPointer)
        {
            if (objectPointer == _srgbPointer) return;
            if (objectPointer == _srgbLinearPointer) return;

            UnmanageAndDispose(objectPointer);
        }

        public object GetNativeColorSpace(IntPtr objectPointer)
        {
            return this[objectPointer];
        }

        public ColorSpaceTransformFn GetTransformFunction(IntPtr objectPointer)
        {
            TryGetInstance(objectPointer, out SKColorSpace skColorSpace);

            if (skColorSpace == null)
            {
                return null;
            }

            SKColorSpaceTransferFn transferFn = skColorSpace.GetNumericalTransferFunction();
            IntPtr nextPointer = functionsCount++;
            _transferFunctions[nextPointer] = transferFn;

            return new ColorSpaceTransformFn(nextPointer);
        }

        public float TransformNumerical(IntPtr objectPointer, float value)
        {
            if (_transferFunctions.TryGetValue(objectPointer, out SKColorSpaceTransferFn transferFn))
            {
                return transferFn.Transform(value);
            }

            throw new InvalidOperationException("Transfer function not found");
        }

        public ColorSpaceTransformFn InvertNumericalTransformFunction(IntPtr objectPointer)
        {
            if (_transferFunctions.TryGetValue(objectPointer, out SKColorSpaceTransferFn transferFn))
            {
                IntPtr nextPointer = functionsCount++;
                _transferFunctions[nextPointer] = transferFn.Invert();

                return new ColorSpaceTransformFn(nextPointer);
            }

            throw new InvalidOperationException("Transfer function not found");
        }

        public float[] GetTransformFunctionValues(IntPtr objectPointer)
        {
            if (_transferFunctions.TryGetValue(objectPointer, out SKColorSpaceTransferFn transferFn))
            {
                return transferFn.Values;
            }

            throw new InvalidOperationException("Transfer function not found");
        }

        public bool IsSrgb(IntPtr objectPointer)
        {
            TryGetInstance(objectPointer, out SKColorSpace skColorSpace);

            return skColorSpace?.IsSrgb ?? false;
        }

        public object GetNativeNumericalTransformFunction(IntPtr objectPointer)
        {
            return _transferFunctions[objectPointer];
        }

        public void DisposeNumericalTransformFunction(IntPtr objectPointer)
        {
            _transferFunctions.Remove(objectPointer);
        }
    }
}
