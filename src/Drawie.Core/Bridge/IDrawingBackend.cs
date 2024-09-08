using Drawie.Core.Bridge.NativeObjectsImpl;
using Drawie.Core.Bridge.Operations;
using Drawie.RenderApi;

namespace Drawie.Core.Bridge
{
    public interface IDrawingBackend
    {
        public void Setup(IRenderApi renderApi);
        public IColorImplementation ColorImplementation { get; }
        public IImageImplementation ImageImplementation { get; }
        public ICanvasImplementation CanvasImplementation { get; }
        public IPaintImplementation PaintImplementation { get; }
        public IVectorPathImplementation PathImplementation { get; }
        public IMatrix3X3Implementation MatrixImplementation { get; }
        public IPixmapImplementation PixmapImplementation { get; }
        public ISurfaceImplementation SurfaceImplementation { get; }
        public IColorSpaceImplementation ColorSpaceImplementation { get; }
        public IImgDataImplementation ImgDataImplementation { get; }
        public IBitmapImplementation BitmapImplementation { get; }
        public IColorFilterImplementation ColorFilterImplementation { get; }
        public IImageFilterImplementation ImageFilterImplementation { get; }
        public IShaderImplementation ShaderImplementation { get; set; }
        public bool IsHardwareAccelerated { get; }
        public IRenderingDispatcher RenderingDispatcher { get; set; }
    }
}
