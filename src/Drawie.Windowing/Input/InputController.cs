namespace Drawie.Windowing.Input;

public class InputController
{
    public IKeyboard? PrimaryKeyboard => Keyboards.FirstOrDefault();
    public IPointer? PrimaryPointer => Pointers.FirstOrDefault();
    public IReadOnlyCollection<IKeyboard> Keyboards { get; }

    public IReadOnlyCollection<IPointer> Pointers { get; }
    
    public InputController(IEnumerable<IKeyboard> keyboards, IEnumerable<IPointer> pointers)
    {
        Keyboards = keyboards.ToList().AsReadOnly();
        Pointers = pointers.ToList().AsReadOnly();
    }
}