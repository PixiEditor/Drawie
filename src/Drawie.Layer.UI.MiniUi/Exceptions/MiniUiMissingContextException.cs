namespace Drawie.Layer.UI.MiniUi.Exceptions;

public class MiniUiMissingContextException : Exception
{ 
    public MiniUiMissingContextException()
        : base("No active MiniUi context is available")
    {
    }
}