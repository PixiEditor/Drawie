using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;

namespace Drawie.Backend.Core.Text;

public class Font : NativeObject
{
    public Font(IntPtr objPtr) : base(objPtr)
    {
    }

    public override object Native => DrawingBackendApi.Current.FontImplementation.GetNative(ObjectPointer);

    public double FontSize
    {
        get => DrawingBackendApi.Current.FontImplementation.GetFontSize(ObjectPointer);
        set => DrawingBackendApi.Current.FontImplementation.SetFontSize(ObjectPointer, value);
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.FontImplementation.Dispose(ObjectPointer);
    }

    public static Font FromStream(Stream stream, float fontSize = 12f, float scaleX = 1f, float skewY = 0f)
    {
        return DrawingBackendApi.Current.FontImplementation.FromStream(stream, fontSize, scaleX, skewY);
    }

    public double MeasureText(string text)
    {
        return DrawingBackendApi.Current.FontImplementation.MeasureText(ObjectPointer, text);
    }

    public static Font CreateDefault(float fontSize = 12f)
    {
        return DrawingBackendApi.Current.FontImplementation.CreateDefault(fontSize);
    }

    public static Font? FromFamilyName(string familyName)
    {
        return DrawingBackendApi.Current.FontImplementation.FromFamilyName(familyName);
    }
}
