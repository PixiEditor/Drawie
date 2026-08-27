namespace Drawie.RenderApi;

public class GpuInfo(string deviceName, string vendor, bool? isDiscreteGpu = null)
{
    public string Name { get; } = deviceName;
    public string Vendor { get; } = vendor;
    public bool? IsDiscreteGpu { get; set; } = isDiscreteGpu;

    public override string ToString()
    {
        return $"{Name} ({Vendor})";
    }
}
