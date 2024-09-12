using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.ImageData;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl
{
    public interface IColorImplementation
    {
        public ColorF ColorToColorF(uint colorValue);
        public Color ColorFToColor(ColorF color);
        public ColorType GetPlatformColorType();
    }
}
