using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.RenderApi;
using Drawie.Windowing;

namespace DrawiEngine;

public class DrawingEngine
{
    public IRenderApi RenderApi { get; }
    public IWindowingPlatform? WindowingPlatform { get; }
    public IDrawingBackend DrawingBackend { get; }
    
    public IRenderingDispatcher RenderingDispatcher { get; } = new DrawieRenderingDispatcher();

    public DrawingEngine(IRenderApi renderApi, IWindowingPlatform? windowingPlatform,
        IDrawingBackend drawingBackend, IRenderingDispatcher renderingDispatcher)
    {
        RenderApi = renderApi;
        WindowingPlatform = windowingPlatform;
        DrawingBackend = drawingBackend;
        RenderingDispatcher = renderingDispatcher;

        DrawingBackendApi.SetupBackend(DrawingBackend, renderingDispatcher);
    }

    public void RunWithApp(DrawieApp app)
    {
        Console.WriteLine("Running DrawieEngine with configuration:");
        Console.WriteLine($"\t- RenderApi: {RenderApi}");
        Console.WriteLine($"\t- WindowingPlatform: {WindowingPlatform}");
        Console.WriteLine($"\t- DrawingBackend: {DrawingBackend}");

        app.Initialize(this);
        IWindow window = app.CreateMainWindow();

        window.Initialize();

        DrawingBackendApi.InitializeBackend(RenderApi);

        app.Run();
        window.Show();
    }

    public void Run()
    {
        Console.WriteLine("Running DrawieEngine with configuration:");
        Console.WriteLine($"\t- RenderApi: {RenderApi}");
        Console.WriteLine($"\t- WindowingPlatform: {WindowingPlatform}");
        Console.WriteLine($"\t- DrawingBackend: {DrawingBackend}");
        
        DrawingBackendApi.InitializeBackend(RenderApi);
    }

    public async ValueTask Dispose()
    {
        await DrawingBackend.DisposeAsync();
    }
}
