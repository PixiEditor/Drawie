using Drawie.Backend.Core.ColorsImpl;

namespace Drawie.Html5Canvas.Extensions;

public static class ColorExtensions
{
    public static string ToCssColor(this Color color)
    {
        return $"rgba({color.R}, {color.G}, {color.B}, {color.A / 255f})";
    }
}