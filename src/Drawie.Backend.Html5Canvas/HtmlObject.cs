using System.Runtime.InteropServices;

namespace Drawie.Html5Canvas;

public class HtmlObject
{
    private static int HandleCounter = 0;
    public int Handle { get; } = HandleCounter++;
}