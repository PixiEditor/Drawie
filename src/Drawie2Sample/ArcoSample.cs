using Drawie.Backend.Arco;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using Drawie.Rendering;

namespace Drawie2Sample;

public abstract class ArcoSample
{
    public ArcoGraphicsContext Context { get; set; }
    public Surface RenderSurface { get; }

    private bool init;
    
    public ArcoSample(ArcoGraphicsContext context, VecI size)
    {
        Context = context;
        RenderSurface = new Surface(context, new ImageInfo {Width =  size.X, Height = size.Y});
    }

    public abstract void OnInit();

    public void Init()
    {
        if (!init)
        {
            OnInit();
            init = true;
        }
    }
}