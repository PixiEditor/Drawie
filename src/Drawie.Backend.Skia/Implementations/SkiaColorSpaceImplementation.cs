using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Surfaces.ImageData;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaColorSpaceImplementation : SkObjectImplementation<SKColorSpace>, IColorSpaceImplementation
    {
        private readonly IntPtr _srgbPointer;
        private readonly IntPtr _srgbLinearPointer;

        public SkiaColorSpaceImplementation()
        {
            _srgbPointer = SKColorSpace.CreateSrgb().Handle;
            _srgbLinearPointer = SKColorSpace.CreateSrgbLinear().Handle;
        }

        public ColorSpace CreateSrgb()
        {
            SKColorSpace skColorSpace = SKColorSpace.CreateSrgb();
            ManagedInstances[skColorSpace.Handle] = skColorSpace;
            return new ColorSpace(skColorSpace.Handle);
        }

        public ColorSpace CreateSrgbLinear()
        {
            SKColorSpace skColorSpace = SKColorSpace.CreateSrgbLinear();
            ManagedInstances[skColorSpace.Handle] = skColorSpace;
            return new ColorSpace(skColorSpace.Handle);
        }

        public void Dispose(IntPtr objectPointer)
        {
            if (objectPointer == _srgbPointer) return;
            if (objectPointer == _srgbLinearPointer) return;

            ManagedInstances[objectPointer].Dispose();
            ManagedInstances.TryRemove(objectPointer, out _);
        }

        public object GetNativeColorSpace(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer];
        }

        public bool IsSrgb(IntPtr objectPointer)
        {
            ManagedInstances.TryGetValue(objectPointer, out SKColorSpace skColorSpace);

            return skColorSpace?.IsSrgb ?? false;
        }
    }
}
