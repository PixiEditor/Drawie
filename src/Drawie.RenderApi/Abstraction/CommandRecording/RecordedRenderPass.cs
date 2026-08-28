namespace Drawie.RenderApi.Abstraction.CommandRecording;

public class RecordedRenderPass
{
    public Action Execute { get; init; }

    public RecordedRenderPass()
    {
        
    }

    public RecordedRenderPass(Action execute)
    {
        Execute = execute;
    }
}