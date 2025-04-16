using Drawie.Backend.Core.Surfaces.ImageData;

namespace Drawie.Skia.Encoders;

public interface IImageEncoder
{
    public byte[] Encode(Image image);
}
