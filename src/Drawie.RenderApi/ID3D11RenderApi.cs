namespace Drawie.RenderApi;

public interface ID3D11RenderApi : IRenderApi
{
    public ID3D11Context D3D11Context { get; }
}
