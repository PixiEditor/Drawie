namespace Drawie.RenderApi;

public interface IWindowRenderApi
{
    public void CreateInstance(object surface);
    public void DestroyInstance();

    public GraphicsApi GraphicsApi { get; }
}