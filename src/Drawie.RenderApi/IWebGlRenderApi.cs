namespace Drawie.RenderApi;

public interface IWebGlRenderApi : IRenderApi
{
    public IWebGlContext WebGlContext { get; }
}
