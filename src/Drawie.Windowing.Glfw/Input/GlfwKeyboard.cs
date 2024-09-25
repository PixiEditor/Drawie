using Drawie.Windowing.Input;
using Silk.NET.Input;
using IKeyboard = Silk.NET.Input.IKeyboard;
using Key = Silk.NET.Input.Key;

namespace Drawie.Silk.Input;

public class GlfwKeyboard : Drawie.Windowing.Input.IKeyboard
{
    public event KeyPress? KeyPressed;
    
    public GlfwKeyboard(IKeyboard silkKeyboard)
    {
        silkKeyboard.KeyDown += OnKeyDown;
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        KeyPressed?.Invoke(this, (Drawie.Windowing.Input.Key) key, keyCode);
    }
}