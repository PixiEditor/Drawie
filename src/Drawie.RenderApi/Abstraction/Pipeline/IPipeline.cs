using System.Collections;

namespace Drawie.RenderApi.Abstraction.Pipeline;

public interface IPipeline
{
    PipelineDesc Description { get; }
    void Apply();
}