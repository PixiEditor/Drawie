using System.Text;
using Drawie.RenderApi;

namespace Drawie.Backend.Core.Debug;

public class GpuDiagnostics
{
    public bool IsHardwareAccelerated { get; }
    public GpuInfo? ActiveGpuInfo { get; }
    public string? RenderApiName { get; }
    public Dictionary<string, string> RenderApiDetails { get; }

    public GpuDiagnostics(bool isHardwareAccelerated, GpuInfo? activeGpuInfo, string? renderApiName,
        Dictionary<string, string> renderApiDetails)
    {
        IsHardwareAccelerated = isHardwareAccelerated;
        ActiveGpuInfo = activeGpuInfo;
        RenderApiName = renderApiName;
        RenderApiDetails = renderApiDetails;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Hardware Accelerated: {IsHardwareAccelerated}");
        builder.AppendLine($"Render API: {RenderApiName}");
        builder.AppendLine($"GPU Info: {ActiveGpuInfo}");
        builder.AppendLine("Details:");
        foreach (var (key, value) in RenderApiDetails)
        {
            builder.AppendLine($"{key}: {value}");
        }

        return builder.ToString();
    }
}
