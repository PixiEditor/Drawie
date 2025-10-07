namespace Drawie.RenderApi;

public interface IDirectXRenderApi : IRenderApi
{
    public IDirectXContext DirectXContext { get; }
}
