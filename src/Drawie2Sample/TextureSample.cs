using Drawie.Backend.Arco;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.Rendering;
using Canvas = Drawie.Backend.Arco.Canvas;

public static class TextureSample
{
    static Canvas cnvs = null;

    public static void Draw(TextureFramebuffer target)
    {
        if (cnvs == null)
        {
            cnvs = new Canvas(DrawingBackendApi.Current.ActiveRenderApi.GraphicsDevice, target.Size);

            Texture tex = Texture.Load("Assets/textures/diffuse.png");
            
            cnvs.DrawSurface(tex, 0, 0, new Paint());

            cnvs.Flush();
        }
        
        cnvs.BlitTo(target);
    }
}