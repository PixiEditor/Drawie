using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using Drawie.RenderApi;

namespace Drawie.Backend.Core.Bridge
{
    public interface IDrawingBackend : IAsyncDisposable
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
        public IPathEffectImplementation PathEffectImplementation { get; }
        public bool IsHardwareAccelerated { get; }
        public IRenderingDispatcher RenderingDispatcher { get; set; }
        public IFontImplementation FontImplementation { get; }
        public IRecorderImplementation RecorderImplementation { get; }
        public IPictureImplementation PictureImplementation { get; }
        public IBlenderImplementation BlenderImplementation { get; }
        public IMeshImplementation MeshImplementation { get; }
        public DrawingSurface CreateRenderSurface(VecI size, ITexture renderTexture, SurfaceOrigin origin);
        public int GetNativeInstancesTotalCount();
        public void Flush();
    }
}
