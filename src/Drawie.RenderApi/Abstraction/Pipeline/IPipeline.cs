using System.Collections;
using Drawie.RenderApi.Abstraction.CommandRecording;

namespace Drawie.RenderApi.Abstraction.Pipeline;

public interface IPipeline
{
    PipelineDesc Description { get; }
    void Apply(ICommandList cmdList);
}