namespace Drawie.RenderApi;

public interface ID3D11Context
{
    public IntPtr Device { get; }
    public IntPtr Adapter { get; }
}
