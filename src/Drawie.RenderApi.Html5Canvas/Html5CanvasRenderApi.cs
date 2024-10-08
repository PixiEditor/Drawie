﻿namespace Drawie.RenderApi.Html5Canvas;

public class Html5CanvasRenderApi : IRenderApi
{
    public Html5CanvasWindowApi WindowRenderApi { get; private set; }
    
    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis => new List<IWindowRenderApi> { WindowRenderApi };
    public IWindowRenderApi CreateWindowRenderApi()
    {
        if (WindowRenderApi != null)
        {
            throw new InvalidOperationException("Window render API was already created.");
        }
        
        WindowRenderApi = new Html5CanvasWindowApi();
        return WindowRenderApi;
    }
}