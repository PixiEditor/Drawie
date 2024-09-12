namespace Drawie.Skia.Exceptions;

public class GrContextAlreadyInitializedException : Exception
{
    public GrContextAlreadyInitializedException() : base("GRContext is already initialized")
    {
    }
}
