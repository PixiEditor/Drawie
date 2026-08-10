namespace Drawie.RenderApi.Abstraction.CommandRecording;

public class RecordedRenderPass
{
    public Action[] Instructions { get; }

    public RecordedRenderPass(Action[] instructions)
    {
        Instructions = instructions;
    }
}