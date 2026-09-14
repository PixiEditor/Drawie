using Drawie.Backend.Arco.RenderingOps;
using Drawie.Backend.Shaders.Common;
using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.Backend.Arco;

public class ArcoGraphicsContext
{
    public IGraphicsDevice Device { get; }
    public IReadOnlyDictionary<RenderOpType, RenderingOpPipeline> RenderingOps => renderingOps;
    public ISampler GlobalSampler { get; }
    
    private Dictionary<RenderOpType, RenderingOpPipeline> renderingOps;

    public ArcoGraphicsContext(IGraphicsDevice graphicsDevice)
    {
        Device = graphicsDevice;
        var instancedRectVertex = ShaderLoader.LoadShader("RectInstancedVertex");
        var instancedRectVertexAA = ShaderLoader.LoadShader("RectInstancedVertexAA");
        var rectFillFragment = ShaderLoader.LoadShader("RectFillFragment");
        var circleFillFragment = ShaderLoader.LoadShader("CircleFillFragment");
        var textureFragment = ShaderLoader.LoadShader("TextureFragment");

        GlobalSampler = graphicsDevice.CreateSampler(new SamplerDesc());

        if (instancedRectVertex == null || rectFillFragment == null || circleFillFragment == null ||
            instancedRectVertexAA == null || textureFragment == null)
            throw new Exception("Unable to load shaders");

        renderingOps = new Dictionary<RenderOpType, RenderingOpPipeline>();

        renderingOps[RenderOpType.Rect] =
            new RenderingOpPipeline(graphicsDevice, instancedRectVertex, rectFillFragment);
        renderingOps[RenderOpType.Circle] =
            new RenderingOpPipeline(graphicsDevice, instancedRectVertexAA, circleFillFragment);

        renderingOps[RenderOpType.Texture] =
            new RenderingOpPipeline(graphicsDevice, instancedRectVertex, textureFragment);
    }
}