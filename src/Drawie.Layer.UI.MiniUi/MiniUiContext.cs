using Drawie.Host.Input;
using Drawie.Numerics;
using Drawie.Rendering;

namespace Drawie.Layer.UI.MiniUi;

public class MiniUiContext : IDisposable
{
    public static MiniUiContext? Active { get; private set; }

    public TextureFramebuffer? Framebuffer { get; private set; }
    public VecF CurrentPosition { get; private set; }
    
    public VecD PointerPosition {get; private set;}
    public InputController InputController { get; private set; }

    public InputState LastState { get; private set; } = new InputState();
    

    public IDisposable MakeActive(TextureFramebuffer fb)
    {
        Active = this;
        Framebuffer = fb;
        CurrentPosition = VecI.Zero;
        return this;
    }

    public void Update(InputController input)
    {
        InputController = input;
        PointerPosition = input.PrimaryPointer?.Position ?? new VecD(-1, -1);
    }

    public void Dispose()
    {
        LastState.Update(InputController);
        Framebuffer = null;
        CurrentPosition = VecI.Zero;
        if (Active == this) Active = null;
    }
}