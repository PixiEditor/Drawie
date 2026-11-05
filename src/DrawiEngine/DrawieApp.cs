using Drawie.Backend.Core.Bridge;
using Drawie.Host;

namespace DrawiEngine;

public abstract class DrawieApp
{
    public DrawingEngine Engine { get; private set; }
    
    public void Initialize(DrawingEngine engine)
    {
        if (Engine != null)
        {
            throw new InvalidOperationException("Engine is already initialized");
        }
        
        Engine = engine;
    }

    public abstract IHost CreateMainWindow();

    public void Run()
    {
        if (DrawingBackendApi.Initialized)
        {
            OnInitialize();
        }
        else
        {
            DrawingBackendApi.OnBackendInitialized += OnInitialize;
        }
    }

    protected abstract void OnInitialize();
}
