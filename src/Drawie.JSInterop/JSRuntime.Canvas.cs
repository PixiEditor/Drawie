using System.Runtime.InteropServices.JavaScript;

namespace Drawie.JSInterop;

public partial class JSRuntime
{
    [JSImport("canvas.openContext", "main.js")]
    public static partial int OpenCanvasContext(string canvasId);
    
    [JSImport("canvas.clearRect", "main.js")]
    public static partial void ClearCanvasRect(int contextHandle, double x, double y, double width, double height);
    
    [JSImport("canvas.fillRect", "main.js")]
    public static partial void FillCanvasRect(int contextHandle, double x, double y, double width, double height);
    
    [JSImport("canvas.strokeRect", "main.js")]
    public static partial void StrokeCanvasRect(int contextHandle, double x, double y, double width, double height);

    [JSImport("canvas.setFillStyle", "main.js")]
    public static partial void SetFillStyle(int contextHandle, string color);
    
    [JSImport("canvas.setStrokeStyle", "main.js")]
    public static partial void SetStrokeStyle(int contextHandle, string color);
}