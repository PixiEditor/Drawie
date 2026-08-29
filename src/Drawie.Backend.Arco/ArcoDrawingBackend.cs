using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.Backend.Arco;

public class ArcoDrawingBackend : IDrawingBackend
{
    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    public IRenderApi ActiveRenderApi { get; }
    public void Setup(IRenderApi renderApi)
    {
        throw new NotImplementedException();
    }

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
    public DrawingSurface? CreateRenderSurface(VecI size, ITexture renderTexture, SurfaceOrigin origin)
    {
        throw new NotImplementedException();
    }

    public int GetNativeInstancesTotalCount()
    {
        throw new NotImplementedException();
    }

    public void Flush()
    {
        throw new NotImplementedException();
    }

    public void ResetContext()
    {
        throw new NotImplementedException();
    }
}