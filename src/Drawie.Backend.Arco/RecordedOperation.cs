using Drawie.Backend.Arco.RenderingOps;
using Drawie.Backend.Core.Surfaces;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.Backend.Arco;

public struct RecordedOperation
{
    public RenderOpType RenderOp { get; set; }
    public BlendMode BlendMode { get; set; }
    public DrawInstance RecordedInstance { get; set; }
    public ITexture Texture { get; set; }
}