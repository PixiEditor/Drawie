using PixiEditor.Numerics;

namespace Drawie.RenderApi;

public interface IWindowRenderApi
{
    public void CreateInstance(object surface, VecI framebufferSize);
    public void DestroyInstance();

    public GraphicsApi GraphicsApi { get; }
    public void UpdateFramebufferSize(int width, int height);
    public void PrepareTextureToWrite();
    public void Render(double deltaTime);
    
    public event Action FramebufferResized;
}