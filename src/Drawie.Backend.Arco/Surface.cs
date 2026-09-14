using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.Backend.Arco;

public class Surface
{
    public ArcoGraphicsContext GraphicsContext { get; }
    
    public Canvas Canvas { get; }
    public RectI DeviceClipSize { get; private set; }
    public ImageInfo ImageInfo { get; }
    
    private IRenderTarget renderTarget;

    public Surface(ArcoGraphicsContext context, ImageInfo imageInfo)
    {
        GraphicsContext = context;
        ImageInfo = imageInfo;
        DeviceClipSize = new RectI(0, 0, imageInfo.Width, imageInfo.Height);
        
        renderTarget = GraphicsContext.Device.CreateRenderTarget(new TextureDesc()
        {
            Depth = DepthFormat.NoDepth,
            Format = TextureFormat.RGBA8_Unorm,
            Width = imageInfo.Width,
            Height = imageInfo.Height,
            Samples = 1,
        });
        
        Canvas = new Canvas(context, renderTarget);
    }
}