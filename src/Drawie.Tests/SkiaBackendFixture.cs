using Drawie.Backend.Core.Bridge;
using Drawie.RenderApi.OpenGL;
using Drawie.RenderApi.Vulkan;
using Drawie.Skia;
using Drawie.Windowing;
using DrawiEngine;
using DrawiEngine.Desktop;

namespace Drawie.Tests;

public class SkiaBackendFixture : IDisposable
{
    public SkiaBackendFixture()
    {
        // TODO: Test context with GrContext
    }

    public void Dispose()
    {
        DrawingBackendApi.Current.DisposeAsync();
    }
}
