namespace Drawie.RenderApi.D3D11;

public class D3D11RenderApi : IDirectXRenderApi
{
    public IDirectXContext DirectXContext { get; }

    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis { get; }
    public IWindowRenderApi CreateWindowRenderApi()
    {
        throw new NotImplementedException();
    }

    public D3D11RenderApi(IDirectXContext directXContext)
    {
        DirectXContext = directXContext;
        WindowRenderApis = [];
    }
}
