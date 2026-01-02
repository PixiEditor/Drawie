namespace Drawie.RenderApi.D3D11;

public class D3D11RenderApi : ID3D11RenderApi
{
    public ID3D11Context D3D11Context { get; }

    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis { get; }

    public IWindowRenderApi CreateWindowRenderApi()
    {
        throw new NotImplementedException();
    }

    public D3D11RenderApi(ID3D11Context d3d11Context)
    {
        D3D11Context = d3d11Context;
    }
}
