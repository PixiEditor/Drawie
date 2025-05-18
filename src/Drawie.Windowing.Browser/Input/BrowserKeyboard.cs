using Drawie.JSInterop;
using Drawie.Windowing.Input;

namespace Drawie.Windowing.Browser.Input;

public class BrowserKeyboard : IKeyboard
{
    private readonly Dictionary<Key, KeyState> _keyStates = new();

    public BrowserKeyboard()
    {
        BrowserInterop.SubscribeKeyDownEvent(OnKeyDown);
        BrowserInterop.SubscribeKeyUpEvent(OnKeyUp);
    }

    public event KeyPress? KeyPressed;

    public bool IsKeyPressed(Key key)
    {
        if (_keyStates.TryGetValue(key, out var state))
        {
            return state == KeyState.Pressed;
        }

        return false;
    }

    private void OnKeyDown(string key)
    {
        if (!Enum.TryParse<Key>(key, true, out var parsedKey))
        {
            Console.WriteLine($"Failed to parse key: {key}");
            return;
        }

        if (_keyStates.TryGetValue(parsedKey, out var state))
        {
            if (state == KeyState.Released)
            {
                _keyStates[parsedKey] = KeyState.Pressed;
                int keyCode = (int)parsedKey; // this might be wrong
                KeyPressed?.Invoke(this, parsedKey, keyCode);
            }
        }
        else
        {
            _keyStates[parsedKey] = KeyState.Pressed;
            KeyPressed?.Invoke(this, parsedKey, (int)parsedKey);
        }
    }

    private void OnKeyUp(string key)
    {
        if (!Enum.TryParse<Key>(key, true, out var parsedKey))
        {
            Console.WriteLine($"Failed to parse key: {key}");
            return;
        }

        if (_keyStates.TryGetValue(parsedKey, out var state))
        {
            if (state == KeyState.Pressed)
            {
                _keyStates[parsedKey] = KeyState.Released;
            }
        }
    }
}

enum KeyState
{
    None,
    Pressed,
    Released
}