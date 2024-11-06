using Drawie.Backend.Core.Text;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IFontImplementation
{
    public object GetNative(IntPtr objectPointer);
    public void Dispose(IntPtr objectPointer);
    public Font FromStream(Stream stream, float fontSize, float scaleX, float skewY);
    public double GetFontSize(IntPtr objectPointer);
    public void SetFontSize(IntPtr objectPointer, double value);
    public double MeasureText(IntPtr objectPointer, string text);
    public Font CreateDefault(float fontSize);
    public Font? FromFamilyName(string familyName);
}
