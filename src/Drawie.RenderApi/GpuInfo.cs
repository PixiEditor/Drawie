namespace Drawie.RenderApi;

public class GpuInfo(string deviceName, string vendor)
{
    public string Name { get; } = deviceName;
    public string Vendor { get; } = vendor;

    public override string ToString()
    {
        return $"{Name} ({Vendor})";
    }
}
