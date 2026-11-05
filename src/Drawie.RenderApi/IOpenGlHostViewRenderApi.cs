namespace Drawie.RenderApi;

public interface IOpenGlHostViewRenderApi : IHostViewRenderApi
{
    Func<string, nint> GetGlInterface();
}
