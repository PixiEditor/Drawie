using System.Numerics;
using Drawie.Backend.Arco.Blending;
using Drawie.Backend.Arco.Buffers;
using Drawie.Backend.Arco.Numerics;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Shaders.Common;
using Drawie.Backend.Vertie.Core;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.Rendering;

namespace Drawie.Backend.Arco;

public class Canvas
{
    public IGraphicsDevice GraphicsDevice { get; }

    private IShaderProgram shaderProgram;
    private ICommandList commandList;
    private GrowableBuffer<RectDrawInstance> instancesBuffer;
    private RecordedOperation[] recordedInstances = new RecordedOperation[256];
    private int recordedInstanceCount;
    private IBuffer<Globals> globalsBuffer;
    private List<NamedBuffer> uniformBlocks;
    private IRenderTarget renderTarget;

    private static Guid pipelineGroupId = Guid.NewGuid();

    private static Dictionary<BlendMode, IPipeline> blendModePipelines = new Dictionary<BlendMode, IPipeline>();

    public Canvas(IGraphicsDevice device, VecI size)
    {
        renderTarget = device.CreateRenderTarget(new TextureDesc()
        {
            Depth = DepthFormat.NoDepth,
            Format = TextureFormat.RGBA8_Unorm,
            Width = size.X,
            Height = size.Y,
            Samples = 1,
        });

        GraphicsDevice = device;
        var instancedRectVertex = ShaderLoader.LoadShader("RectInstancedVertex");
        var fillFragment = ShaderLoader.LoadShader("FillFragment");

        if (instancedRectVertex == null || fillFragment == null) throw new Exception("Unable to load shaders");

        shaderProgram =
            GraphicsDevice.CreateShaderProgram(new ShaderProgramDesc([
                instancedRectVertex, fillFragment
            ]));

        CreatePipelineForBlendMode(size, BlendMode.SrcOver);

        instancesBuffer = new GrowableBuffer<RectDrawInstance>(GraphicsDevice, BufferUsage.Storage);
        globalsBuffer = GraphicsDevice.CreateBuffer<Globals>(BufferUsage.Uniform, [
            new() { ViewportSize = renderTarget.Size.ToVector2() }
        ]);

        globalsBuffer.SetData([new Globals { ViewportSize = renderTarget.Size.ToVector2() }]);

        uniformBlocks = new List<NamedBuffer>(2)
        {
            new NamedBuffer("instances", instancesBuffer.Buffer),
            new NamedBuffer("globals", globalsBuffer)
        };
    }

    public void DrawRect(float x, float y, float width, float height, Paint paint)
    {
        if (recordedInstanceCount == recordedInstances.Length)
        {
            Array.Resize(ref recordedInstances, recordedInstances.Length * 2);
        }

        var fill = paint.Color;

        recordedInstances[recordedInstanceCount++] = new()
        {
            RecordedInstance = new RectDrawInstance()
            {
                Color = new Vector4(fill.R / 255f, fill.G / 255f, fill.B / 255f, fill.A / 255f),
                Position = new Vector2(x, y),
                Size = new Vector2(width, height)
            },

            BlendMode = paint.BlendMode
        };
    }

    public void Flush(TextureFramebuffer? blitTo = null)
    {
        if (recordedInstanceCount == 0) return;

        commandList = GraphicsDevice.CreateCommandList();
        
        BlendMode currentBlendMode = recordedInstances[0].BlendMode;
        int batchStart = 0;

        bool renderPassStarted = false;

        for (int i = 1; i < recordedInstanceCount; i++)
        {
            if (recordedInstances[i].BlendMode != currentBlendMode)
            {
                DrawBatch(currentBlendMode, batchStart, i - batchStart, renderPassStarted);
                renderPassStarted = true;
                currentBlendMode = recordedInstances[i].BlendMode;
                batchStart = i;
            }
        }

        DrawBatch(currentBlendMode, batchStart, recordedInstanceCount - batchStart, renderPassStarted);

        var recordedRenderPass = commandList.EndRenderPass(blitTo);
        GraphicsDevice.Submit(recordedRenderPass);

        recordedInstanceCount = 0;
    }

    private void BeginRender()
    {
        commandList.BeginRenderPass(renderTarget);
        
        instancesBuffer.SetData(recordedInstances.Take(recordedInstanceCount).Select(x => x.RecordedInstance)
            .ToArray());
        uniformBlocks[0].Buffer = instancesBuffer.Buffer;

        commandList.UpdateUniforms(uniformBlocks);
    }

    private void DrawBatch(BlendMode blendMode, int at, int count, bool renderPassStarted)
    {
        if (!blendModePipelines.ContainsKey(blendMode))
        {
            CreatePipelineForBlendMode(renderTarget.Size, blendMode);
        }

        commandList.SetPipeline(blendModePipelines[blendMode]);

        if (!renderPassStarted)
        {
            BeginRender();
        }
        
        commandList.BindPipeline();
        
        commandList.Draw(6, at, count);
    }


    private void CreatePipelineForBlendMode(VecI size, BlendMode blendMode)
    {
        blendModePipelines[blendMode] = GraphicsDevice.CreatePipeline(new PipelineDesc()
        {
            Depth = new DepthDesc()
            {
                Enabled = false,
            },
            Rasterizer = new RasterizerDesc()
            {
                RenderMode = RenderMode.Default,
                Samples = 1,
                CullMode = CullMode.None
            },
            Blend = new BlendDesc()
            {
                Preset = blendMode.ToBlendingPreset()
            },
            RenderPass = new RenderPassDesc()
            {
                ColorLoadOp = ColorLoadOp.Load
            },
            ShaderProgram = shaderProgram,
            Viewport = new RectI(0, 0, size.X, size.Y),
            PipelineVariantGroupId = pipelineGroupId
        });
    }

    public void BlitTo(TextureFramebuffer target)
    {
        commandList = GraphicsDevice.CreateCommandList();
        commandList.Blit(renderTarget, target);
        GraphicsDevice.Submit(commandList.End());
    }
}