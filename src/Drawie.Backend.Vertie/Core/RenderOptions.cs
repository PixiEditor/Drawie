namespace Drawie.Backend.Vertie.Core;

public struct RenderOptions
{
    public RenderMode RenderMode { get; set; }
    public MsaaSamples MsaaSamples { get; set; }
}

public enum MsaaSamples
{
    None = 0,
    X2 = 2,
    X4 = 4,
    X8 = 8,
    X16 = 16,
}