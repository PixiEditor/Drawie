using Drawie.RenderApi.Abstraction.Buffers;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlBufferList : IBufferGroupList
{
    public List<IBuffer> Buffers { get; } = new List<IBuffer>();
}