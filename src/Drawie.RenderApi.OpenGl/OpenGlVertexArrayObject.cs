using Drawie.RenderApi.Abstraction.Buffers;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlVertexArrayObject : IBufferGroup
{
    public uint Handle { get; }

    public GL Api { get; }

    private OpenGlBufferList bufferList;

    public OpenGlVertexArrayObject(GL api)
    {
        Api = api;
        Handle = Api.GenVertexArray();
        bufferList = new OpenGlBufferList();
    }

    public void Open(Action<IBufferGroupList> list)
    {
        Api.BindVertexArray(Handle);
        list(bufferList);
        Api.BindVertexArray(0);
    }
}