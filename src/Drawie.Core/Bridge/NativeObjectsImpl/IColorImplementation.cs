using Drawie.Core.ColorsImpl;
using Drawie.Core.Surfaces.ImageData;

namespace Drawie.Core.Bridge.NativeObjectsImpl
{
    public interface IColorImplementation
    {
        public ColorF ColorToColorF(uint colorValue);
        public Color ColorFToColor(ColorF color);
        public ColorType GetPlatformColorType();
    }
}
