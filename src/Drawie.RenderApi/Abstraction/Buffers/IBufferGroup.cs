namespace Drawie.RenderApi.Abstraction.Buffers;

public interface IBufferGroup
{
    public uint Handle { get; }
    void Open(Action<IBufferGroupList> list);
}