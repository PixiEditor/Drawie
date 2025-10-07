namespace Drawie.RenderApi;

public interface IDirectXContext
{
    public IntPtr Adapter { get; }
    public IntPtr Device { get; }
    public IntPtr Queue { get; }
}
