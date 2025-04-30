using Drawie.Numerics;

namespace Drawie.Windowing.Input;

public delegate void PointerPress(IPointer pointer, PointerButton button, VecD position);
public delegate void PointerRelease(IPointer pointer, PointerButton button, VecD position);
public delegate void PointerMove(IPointer pointer, VecD position);
public delegate void PointerClick(IPointer pointer, PointerButton button, VecD position);
public delegate void PointerDoubleClick(IPointer pointer, PointerButton button, VecD position);
public delegate void PointerScroll(IPointer pointer, VecD scrollDelta);

public interface IPointer
{
    public event PointerPress PointerPressed;
    public event PointerRelease PointerReleased;
    public event PointerMove PointerMoved;
    public event PointerClick PointerClicked;
    public event PointerDoubleClick PointerDoubleClicked;
    public event PointerScroll PointerScrolled;
    public VecD Position { get; }

    public bool IsButtonPressed(PointerButton button);
}

public enum PointerButton
{
    /// <summary>
    /// Indicates the input backend was unable to determine a button name for the button in question, or it does not support it.
    /// </summary>
    Unknown = -1,

    /// <summary>
    /// The left mouse button.
    /// </summary>
    Left = 0,

    /// <summary>
    /// The right mouse button.
    /// </summary>
    Right,

    /// <summary>
    /// The middle mouse button.
    /// </summary>
    Middle,

    /// <summary>
    /// The fourth mouse button.
    /// </summary>
    Button4,

    /// <summary>
    /// The fifth mouse button.
    /// </summary>
    Button5,

    /// <summary>
    /// The sixth mouse button.
    /// </summary>
    Button6,

    /// <summary>
    /// The seventh mouse button.
    /// </summary>
    Button7,

    /// <summary>
    /// The eighth mouse button.
    /// </summary>
    Button8,

    /// <summary>
    /// The ninth mouse button.
    /// </summary>
    Button9,

    /// <summary>
    /// The tenth mouse button.
    /// </summary>
    Button10,

    /// <summary>
    /// The eleventh mouse button.
    /// </summary>
    Button11,

    /// <summary>
    /// The twelth mouse button.
    /// </summary>
    Button12
}