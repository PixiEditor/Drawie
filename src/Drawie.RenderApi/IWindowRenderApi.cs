using Drawie.Numerics;

namespace Drawie.RenderApi;

public interface IWindowRenderApi
{
    public IGraphicsContext GraphicsContext { get; }
    public void CreateInstance(object contextObject, VecI framebufferSize);
    public void DestroyInstance();

    public void UpdateFramebufferSize(int width, int height);
    public void PrepareTextureToWrite();
    public void Render(double deltaTime);
    
    public event Action FramebufferResized;
   
    public ITexture RenderTexture { get; }
}