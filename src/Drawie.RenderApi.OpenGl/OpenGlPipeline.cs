using System.Drawing;
using Drawie.RenderApi.Abstraction.Pipeline;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlPipeline : IPipeline
{
    public GL Api { get; }
    public PipelineDesc Description { get; }

    public OpenGlPipeline(PipelineDesc description, GL api)
    {
        Description = description;
        Api = api;
    }

    public void Apply()
    {
        Api.Viewport(new Rectangle(Description.Viewport.X, Description.Viewport.Y, Description.Viewport.Width,
            Description.Viewport.Height));

        if (Description.Blend.Enabled)
        {
            Api.Enable(EnableCap.Blend);
        }
        else
        {
            Api.Disable(EnableCap.Blend);
        }

        if (Description.Depth.Enabled)
        {
            Api.Enable(EnableCap.DepthTest);
            Api.DepthFunc(ToOpenGlDesc(Description.Depth.DepthCompare));
            Api.DepthMask(true);
            
            Api.ClearDepth(1.0);
        }
        else
        {
            Api.Disable(EnableCap.DepthTest);
        }
        
        Api.Disable(EnableCap.CullFace);
        Api.ClearColor(0, 0, 0, 1);
        Api.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        Description.ShaderProgram?.Use();
    }

    private DepthFunction ToOpenGlDesc(DepthCompareType depthDepthCompare)
    {
        return depthDepthCompare switch
        {
            DepthCompareType.Less => DepthFunction.Less,
            DepthCompareType.LessEqual => DepthFunction.Lequal,
            DepthCompareType.Equal => DepthFunction.Equal,
            DepthCompareType.Greater => DepthFunction.Greater,
            DepthCompareType.GreaterEqual => DepthFunction.Gequal,
            DepthCompareType.Always => DepthFunction.Always,
            _ => throw new ArgumentOutOfRangeException(nameof(depthDepthCompare), depthDepthCompare, null)
        };
    }
}