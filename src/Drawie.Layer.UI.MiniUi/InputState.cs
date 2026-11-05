using Drawie.Host.Input;

namespace Drawie.Layer.UI.MiniUi;

public class InputState
{
    // public IReadOnlyDictionary<Key, bool> PressedKeys => pressedKeys;
    public IReadOnlyDictionary<PointerButton, bool> PressedPointerButtons => pressedPointerButtons;

    //private Dictionary<Key, bool> pressedKeys = new();
    private Dictionary<PointerButton, bool> pressedPointerButtons = new Dictionary<PointerButton, bool>();

    public InputState()
    {
        int enumRange = Enum.GetValues(typeof(PointerButton)).Length - 1; // - 1 because one state is unknown 
        for (int i = 0; i < enumRange; i++)
        {
            pressedPointerButtons.Add((PointerButton)i, false);
        }
    }

    public void Update(InputController input)
    {
        //pressedKeys.Clear();
        pressedPointerButtons.Clear();

        int enumRange = Enum.GetValues(typeof(PointerButton)).Length - 1; // - 1 because one state is unknown 
        for (int i = 0; i < enumRange; i++)
        {
            pressedPointerButtons.Add((PointerButton)i, input.PrimaryPointer.IsButtonPressed((PointerButton)i));
        }
    }
}