using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Surfaces.ImageData;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public sealed class SkiaImgDataImplementation : SkObjectImplementation<SKData>, IImgDataImplementation
    {
        public void Dispose(IntPtr objectPointer)
        {
            UnmanageAndDispose(objectPointer);
        }

        public void SaveTo(ImgData imgData, FileStream stream)
        {
            SKData data = this[imgData.ObjectPointer];
            data.SaveTo(stream);
        }

        public Stream AsStream(ImgData imgData)
        {
            SKData data = this[imgData.ObjectPointer];
            return data.AsStream();
        }

        public ReadOnlySpan<byte> AsSpan(ImgData imgData)
        {
            SKData data = this[imgData.ObjectPointer];
            return data.AsSpan();
        }
        
        public ImgData Create(ReadOnlySpan<byte> buffer)
        {
            SKData data = SKData.CreateCopy(buffer.ToArray());
            AddManagedInstance(data);
            return new ImgData(data.Handle);
        }

        public object GetNativeImgData(IntPtr objectPointer)
        {
            return this[objectPointer];
        }

        public void SaveTo(ImgData imgData, Stream stream)
        {
            SKData data = this[imgData.ObjectPointer];
            data.SaveTo(stream);
        }
    }
}
