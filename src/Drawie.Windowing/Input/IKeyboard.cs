namespace Drawie.Windowing.Input;

public delegate void KeyPress(IKeyboard keyboard, Key key, int keyCode);
public interface IKeyboard
{
    public event KeyPress KeyPressed;
    public bool IsKeyPressed(Key key);
}