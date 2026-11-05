using Drawie.Layer.UI.MiniUi.Exceptions;
using Drawie.Numerics;
using Drawie.Rendering;

namespace Drawie.Layer.UI.MiniUi.Controls;

public static class Layout
{
    private static Stack<MeasurementSession> sessions = new Stack<MeasurementSession>();
    
    public static void BeginMeasure()
    {
        MiniUiContext? ctx = MiniUiContext.Active;
        
        if (ctx == null)
            throw new MiniUiMissingContextException();
        
        sessions.Push(new MeasurementSession(ctx.CurrentPosition, ctx.Framebuffer));
        ctx.Framebuffer = null;
    }

    public static RectD EndMeasure()
    {
        MiniUiContext? ctx = MiniUiContext.Active;
        
        if (ctx == null)
            throw new MiniUiMissingContextException();
        
        var session = sessions.Pop();
        RectD rect = RectD.FromTwoPoints((VecD)session.Cursor, (VecD)ctx.CurrentPosition);
        ctx.CurrentPosition = session.Cursor;
        ctx.Framebuffer = session.SavedFramebuffer;
        return rect;
    }
    
    private struct MeasurementSession
    {
        public VecF Cursor { get; set; }
        public TextureFramebuffer SavedFramebuffer { get; set; }

        public MeasurementSession(VecF cursor, TextureFramebuffer savedFramebuffer)
        {
            Cursor = cursor;
            SavedFramebuffer = savedFramebuffer;
        }
    }
}
