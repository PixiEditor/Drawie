using Drawie.Html5Canvas.Impl;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.Surfaces;
using Drawie.Html5Canvas.Objects;
using Drawie.Numerics;
using Drawie.RenderApi;

namespace Drawie.Html5Canvas;

public class HtmlCanvasDrawingBackend : IDrawingBackend
{
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
    public bool IsHardwareAccelerated { get; } = true;
    public IRenderingDispatcher RenderingDispatcher { get; set; }
    
    public HtmlCanvasDrawingBackend()
    {
        Html5CanvasImpl canvasImpl = new Html5CanvasImpl();
        CanvasImplementation = canvasImpl;
        SurfaceImplementation = new Html5CanvasSurface(canvasImpl);
        PaintImplementation = new Html5PaintImpl();
    }

    public void Setup(IRenderApi renderApi)
    {
        
    }

    public DrawingSurface CreateRenderSurface(VecI size, ITexture renderTexture)
    {
        if(renderTexture is ICanvasTexture canvasTexture)
        {
            HtmlCanvasObject canvasObject = new HtmlCanvasObject(canvasTexture.CanvasId, size); 
            return DrawingSurface.FromNative(canvasObject);
        }
        
        throw new ArgumentException($"Unsupported render API: {renderTexture}.");
    }
}