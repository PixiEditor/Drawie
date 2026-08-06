namespace Drawie.RenderApi;

public interface IOpenGlWindowRenderApi : IWindowRenderApi
{
    Func<string, nint> GetGlInterface();
}
