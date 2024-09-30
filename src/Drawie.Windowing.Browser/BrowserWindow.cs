using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.Windowing.Input;

namespace Drawie.Windowing.Browser;

public class BrowserWindow(IWindowRenderApi windowRenderApi) : IWindow
{
    public string Name
    {
        get => BrowserInterop.GetTitle();
        set => BrowserInterop.SetTitle(value);
    }

    public VecI Size { get; set; }

    public VecI UsableWindowSize => BrowserInterop.GetWindowSize();
    
    public IWindowRenderApi RenderApi { get; set; } = windowRenderApi;

    public InputController InputController => throw new NotImplementedException();
    public event Action<double>? Update;
    public event Action<Texture, double>? Render;

    private Texture renderTexture;

    public void Initialize()
    {
        RenderApi.CreateInstance(null, UsableWindowSize);
    }

    public void Show()
    {
        renderTexture = CreateRenderTexture();
        BrowserInterop.RequestAnimationFrame(OnRender);
    }

    private void OnRender(double dt)
    {
        RenderApi.PrepareTextureToWrite();
        renderTexture.DrawingSurface?.Canvas.Clear();
        Render?.Invoke(renderTexture, dt);
        //renderTexture.DrawingSurface?.Flush();
        
        BrowserInterop.RequestAnimationFrame(OnRender);
    }

    public void Close()
    {
    }

    private Texture CreateRenderTexture()
    {
        var drawingSurface = DrawingBackendApi.Current.CreateRenderSurface(UsableWindowSize, RenderApi);
        return Texture.FromExisting(drawingSurface);
    }
}