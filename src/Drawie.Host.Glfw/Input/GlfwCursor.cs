using Drawie.Host.Input;
using Silk.NET.Input;
using ICursor = Drawie.Host.Input.ICursor;

namespace Drawie.Silk.Input;

public class GlfwCursor : ICursor
{
    public global::Silk.NET.Input.ICursor SilkCursor { get; }

    public GlfwCursor(global::Silk.NET.Input.ICursor silkMouseCursor)
    {
        SilkCursor = silkMouseCursor;
    }

    public CursorState State
    {
        get => ToCursorState(SilkCursor.CursorMode);
        set => SilkCursor.CursorMode = ToCursorMode(value);
    }

    private CursorMode ToCursorMode(CursorState value)
    {
        return (CursorMode)value;
    }

    private CursorState ToCursorState(CursorMode silkCursorCursorMode)
    {
        return (CursorState)silkCursorCursorMode;
    }
}