using System.Drawing;
using System.Windows.Input;
using Drawie.Backend.Vertie.Core;
using Drawie.RenderApi.Abstraction.CommandRecording;
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

    public void Apply(ICommandList list)
    {
        if (Description.StaticViewport != null)
        {
            Api.Viewport(new Rectangle(Description.StaticViewport.Value.X, Description.StaticViewport.Value.Y,
                Description.StaticViewport.Value.Width, Description.StaticViewport.Value.Height));
        }

        if (Description.Blend.Preset != BlendingPreset.None && Description.Blend.Preset != BlendingPreset.Src)
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

        if (Description.Rasterizer.Samples != 1)
        {
            Api.Enable(EnableCap.Multisample);
        }
        
        Api.Enable(EnableCap.CullFace);
        Api.CullFace(TriangleFace.Back);
        Api.ClearColor(0, 0, 0, 1);
        Api.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        Api.PolygonMode(TriangleFace.FrontAndBack, Description.Rasterizer.RenderMode == RenderMode.Wireframe ? PolygonMode.Line : PolygonMode.Fill);

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
            DepthCompareType.Never => DepthFunction.Never,
            DepthCompareType.NotEqual => DepthFunction.Notequal,    
            _ => throw new ArgumentOutOfRangeException(nameof(depthDepthCompare), depthDepthCompare, null)
        };
    }
}