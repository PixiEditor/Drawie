using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Drawie.RenderApi.WebGl;

public class WebGlContext : IWebGlContext
{
    public WebGlWindowRenderApi WebGlWindowRenderApi { get; }
    
    public WebGlContext(WebGlWindowRenderApi webGlWindowRenderApi)
    {
        WebGlWindowRenderApi = webGlWindowRenderApi;
    }
    
    public IntPtr GetGlInterface(string name)
    {
        return (IntPtr)1;
        //return JSInterop.JSRuntime.GetProcAddress(name);
    }
}
