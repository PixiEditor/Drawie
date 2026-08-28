using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Drawie.RenderApi.WebGl;

public class WebGlContext : IWebGlContext
{
    public WebGlHostViewRenderApi WebGlHostViewRenderApi { get; }
    
    public WebGlContext(WebGlHostViewRenderApi webGlHostViewRenderApi)
    {
        WebGlHostViewRenderApi = webGlHostViewRenderApi;
    }
    
    public IntPtr GetGlInterface(string name)
    {
        return (IntPtr)1;
        //return JSInterop.JSRuntime.GetProcAddress(name);
    }
}
