namespace Drawie.Skia.Exceptions;

public class RenderApiNotInitializedException : Exception
{
    public RenderApiNotInitializedException() : base("Render API is not initialized.")
    {
    }
}