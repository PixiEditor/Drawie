using Drawie.Core.Bridge;
using Drawie.Windowing;

namespace DrawiEngine;

public class DrawingEngine
{
     public IWindowingPlatform WindowingPlatform { get; } 
     public IDrawingBackend DrawingBackend { get; }
     
     public DrawingEngine(IWindowingPlatform windowingPlatform, IDrawingBackend drawingBackend)
     {
          WindowingPlatform = windowingPlatform;
          DrawingBackend = drawingBackend;
     }
}
