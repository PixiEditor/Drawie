using Drawie.Backend.Vertie.Core;
using Drawie.JSInterop;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.WebGl.Enums;

namespace Drawie.RenderApi.WebGl;

public class WebGlPipeline : IPipeline
{
    public PipelineDesc Description { get; }
    public int Gl { get; }

    public WebGlPipeline(PipelineDesc description, int gl)
    {
        Description = description;
        Gl = gl;
    }

    public void Apply(VulkanCommandList vulkanCommandList)
    {
        JSRuntime.Viewport(Gl, Description.StaticViewport.X, Description.StaticViewport.Y, Description.StaticViewport.Width, Description.StaticViewport.Height);
        
        if (Description.Blend.Enabled)
        {
            JSRuntime.Enable(Gl, (int)WebGlCap.Blend);
        }
        else
        {
            JSRuntime.Disable(Gl, (int)WebGlCap.Blend);
        }

        if (Description.Depth.Enabled)
        {
            JSRuntime.Enable(Gl, (int)WebGlCap.DepthTest);
            JSRuntime.DepthFunc(Gl, ToWebGlDesc(Description.Depth.DepthCompare));
            JSRuntime.DepthMask(Gl, true);
            
            JSRuntime.ClearDepth(Gl, 1.0);
        }
        else
        {
            JSRuntime.Disable(Gl, (int)WebGlCap.DepthTest);
        }
        
        JSRuntime.ClearColor(Gl, 0, 0, 0, 1);
        JSRuntime.Clear(Gl, (int)(WebGlBufferMask.ColorBufferBit | WebGlBufferMask.DepthBufferBit | WebGlBufferMask.StencilBufferBit));

        Description.ShaderProgram?.Use();
    }

    private int ToWebGlDesc(DepthCompareType depthDepthCompare)
    {
        return depthDepthCompare switch
        {
            DepthCompareType.Never => (int)WebGlDepthFunc.Never,
            DepthCompareType.Less => (int)WebGlDepthFunc.Less,
            DepthCompareType.Equal => (int)WebGlDepthFunc.Equal,
            DepthCompareType.LessEqual => (int)WebGlDepthFunc.LEqual,
            DepthCompareType.Greater => (int)WebGlDepthFunc.Greater,
            DepthCompareType.NotEqual => (int)WebGlDepthFunc.NotEqual,
            DepthCompareType.GreaterEqual => (int)WebGlDepthFunc.GEqual,
            DepthCompareType.Always => (int)WebGlDepthFunc.Always,
            _ => throw new ArgumentOutOfRangeException(nameof(depthDepthCompare), depthDepthCompare, null)
        };
    }
}