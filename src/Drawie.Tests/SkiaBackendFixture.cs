using Drawie.Backend.Core.Bridge;

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
