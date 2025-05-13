using Drawie.Windowing.Input;

namespace Drawie.Windowing.Browser.Input;

public class BrowserKeyboard : IKeyboard
{
    public event KeyPress? KeyPressed;
    public bool IsKeyPressed(Key key)
    {
        return BrowserInterop.IsKeyPressed(key);
    }
}