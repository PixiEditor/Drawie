using Drawie.Host.Input;
using Drawie.Numerics;

namespace Drawie.Host.Browser.Input;

public class BrowserPointer : IPointer
{
    public event PointerPress? PointerPressed;
    public event PointerRelease? PointerReleased;
    public event PointerMove? PointerMoved;
    public event PointerClick? PointerClicked;
    public event PointerDoubleClick? PointerDoubleClicked;
    public event PointerScroll? PointerScrolled;
    public VecD Position { get; }
    public ICursor Cursor { get; } = new BrowserCursor();
    
    public bool IsButtonPressed(PointerButton button)
    {
        return false;
    }
}