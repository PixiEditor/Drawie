using Drawie.Backend.Arco;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Rendering;

namespace Drawie2Sample;

public static class Sandbox
{
    static Drawie.Backend.Arco.Canvas cnvs = null;

    public static void Draw(TextureFramebuffer fb)
    {
        if (cnvs == null)
        {
            cnvs = new Canvas(DrawingBackendApi.Current.ActiveRenderApi.GraphicsDevice, fb.Size);
            cnvs.DrawCircle(0, 0, 300, 100, new Paint()
            {
                Color = Colors.Green
            });
            cnvs.Flush();
        }
        
        cnvs.BlitTo(fb);
    }
}