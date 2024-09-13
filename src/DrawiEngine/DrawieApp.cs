using Drawie.Windowing;

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

    public abstract IWindow CreateMainWindow();
    public abstract void Run();
}