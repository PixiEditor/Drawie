using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Surfaces.ImageData;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaColorSpaceImplementation : SkObjectImplementation<SKColorSpace>, IColorSpaceImplementation
    {
        private readonly IntPtr _srgbPointer;
        
        public SkiaColorSpaceImplementation()
        {
            _srgbPointer = SKColorSpace.CreateSrgb().Handle;
        }
        
        public ColorSpace CreateSrgb()
        {
            SKColorSpace skColorSpace = SKColorSpace.CreateSrgb();
            ManagedInstances[skColorSpace.Handle] = skColorSpace;
            return new ColorSpace(skColorSpace.Handle);
        }

        public void Dispose(IntPtr objectPointer)
        {
            if (objectPointer == _srgbPointer) return;
            ManagedInstances[objectPointer].Dispose();
            ManagedInstances.TryRemove(objectPointer, out _);
        }

        public object GetNativeColorSpace(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer];
        }
    }
}
