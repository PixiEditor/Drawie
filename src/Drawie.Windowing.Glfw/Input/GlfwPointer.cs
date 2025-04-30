using System.Numerics;
using Drawie.Numerics;
using Drawie.Windowing.Input;
using Silk.NET.Input;

namespace Drawie.Silk.Input;

public class GlfwPointer : Drawie.Windowing.Input.IPointer
{
    public IMouse SilkMouse { get; }
    public event PointerPress? PointerPressed;
    public event PointerRelease? PointerReleased;
    public event PointerMove? PointerMoved;
    public event PointerClick? PointerClicked;
    public event PointerDoubleClick? PointerDoubleClicked;
    public event PointerScroll? PointerScrolled;
    public VecD Position => new VecD(SilkMouse.Position.X, SilkMouse.Position.Y);

    public GlfwPointer(IMouse silkMouse)
    {
        SilkMouse = silkMouse;
        silkMouse.MouseDown += OnMouseDown;
        silkMouse.MouseUp += OnMouseUp;
        silkMouse.MouseMove += OnMouseMove;
        silkMouse.Click += OnMouseClick;
        silkMouse.DoubleClick += OnMouseDoubleClick;
        silkMouse.Scroll += OnMouseScroll;
    }

    private void OnMouseDown(IMouse mouse, MouseButton button)
    {
        PointerPressed?.Invoke(this, (PointerButton)button, Position);
    }

    private void OnMouseUp(IMouse mouse, MouseButton button)
    {
        PointerReleased?.Invoke(this, (PointerButton)button, Position);
    }

    private void OnMouseMove(IMouse mouse, Vector2 position)
    {
        PointerMoved?.Invoke(this, new VecD(position.X, position.Y));
    }

    private void OnMouseClick(IMouse mouse, MouseButton button, Vector2 position)
    {
        PointerClicked?.Invoke(this, (PointerButton)button, new VecD(position.X, position.Y));
    }

    private void OnMouseDoubleClick(IMouse mouse, MouseButton button, Vector2 position)
    {
        PointerDoubleClicked?.Invoke(this, (PointerButton)button, new VecD(position.X, position.Y));
    }

    private void OnMouseScroll(IMouse mouse, ScrollWheel scrollWheel)
    {
        PointerScrolled?.Invoke(this, new VecD(scrollWheel.X, scrollWheel.Y));
    }

    public bool IsButtonPressed(PointerButton button)
    {
        return SilkMouse.IsButtonPressed((MouseButton)button);
    }
}