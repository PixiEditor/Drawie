namespace Drawie.Windowing.Input;

public class InputController
{
    public IKeyboard? DefaultKeyboard => Keyboards.FirstOrDefault();
    public IReadOnlyCollection<IKeyboard> Keyboards { get; }
    
    public InputController(IEnumerable<IKeyboard> keyboards)
    {
        Keyboards = keyboards.ToList().AsReadOnly();
    }
}