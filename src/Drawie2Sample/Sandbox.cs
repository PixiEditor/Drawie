using Drawie.Backend.Arco;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;
using Drawie.Rendering;

namespace Drawie2Sample;

public static class Sandbox
{
    static Drawie.Backend.Arco.Canvas cnvs = null;

    public static void Draw(TextureFramebuffer fb)
    {
        if (cnvs == null)
        {
            cnvs = new Canvas(DrawingBackendApi.Current.ActiveRenderApi.GraphicsDevice, new VecI(fb.Size.X, fb.Size.Y));
            cnvs.DrawCircle(400, 400, 200, new Paint()
            {
                Color = Colors.Green,
                IsAntiAliased = false
            });
            
            
            cnvs.DrawCircle(850, 400, 200, new Paint()
            {
                Color = Colors.Green,
                IsAntiAliased = true
            });
            
            cnvs.Flush();
        }
        
        cnvs.BlitTo(fb);
    }
}