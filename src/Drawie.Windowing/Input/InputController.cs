namespace Drawie.Windowing.Input;

public class InputController
{
    public IKeyboard? PrimaryKeyboard => Keyboards.FirstOrDefault();
    public IPointer? PrimaryPointer => Pointers.FirstOrDefault();
    public IReadOnlyList<IKeyboard> Keyboards { get; }

    public IReadOnlyList<IPointer> Pointers { get; }
    
    public object NativeInputController { get; }
    
    public InputController(IEnumerable<IKeyboard> keyboards, IEnumerable<IPointer> pointers, object nativeInputController)
    {
        Keyboards = keyboards.ToList().AsReadOnly();
        Pointers = pointers.ToList().AsReadOnly();
        NativeInputController = nativeInputController;
    }
}