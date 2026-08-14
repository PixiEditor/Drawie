using Drawie.JSInterop;
using Drawie.RenderApi.Abstraction.Buffers;

namespace Drawie.RenderApi.WebGl;

public class WebGlVertexArray : IBufferGroup
{
    public uint Handle { get; }
    public int Gl { get; }

    private WebGlBufferGroupList bufferList = new WebGlBufferGroupList();

    public WebGlVertexArray(int glHandle)
    {
        Gl = glHandle;
        Handle = (uint)JSRuntime.CreateVertexArray(Gl);
    }
    
    public void Open(Action<IBufferGroupList> list)
    {
        JSRuntime.BindVertexArray(Gl, (int)Handle);
        list(bufferList);
        JSRuntime.BindVertexArray(Gl, 0);
    }
}

public class WebGlBufferGroupList : IBufferGroupList
{
    public List<IBuffer> Buffers { get; } = new List<IBuffer>();
}