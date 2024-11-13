namespace Drawie.RenderApi;

public interface IOpenGlRenderApi : IRenderApi
{
    public IOpenGlContext OpenGlContext { get; }
}
