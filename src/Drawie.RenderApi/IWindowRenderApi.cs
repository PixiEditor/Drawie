using Drawie.Numerics;

namespace Drawie.RenderApi;

public interface IWindowRenderApi
{
    public void CreateInstance(object surface, VecI framebufferSize);
    public void DestroyInstance();

    public void UpdateFramebufferSize(int width, int height);
    public void PrepareTextureToWrite();
    public void Render(double deltaTime);
    public void InitializeOverlayDebugger();
    
    public event Action FramebufferResized;
}