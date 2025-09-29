using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IFontImplementation
{
    public object GetNative(IntPtr objectPointer);
    public void Dispose(IntPtr objectPointer);
    public Font? FromStream(Stream stream, float fontSize, float scaleX, float skewY);
    public double GetFontSize(IntPtr objectPointer);
    public void SetFontSize(IntPtr objectPointer, double value);
    public double MeasureText(IntPtr objectPointer, string text);
    public Font CreateDefault(float fontSize);
    public Font? FromFamilyName(string familyName, FontStyleWeight weight, FontStyleWidth width, FontStyleSlant slant);
    public Font? FromFamilyName(string familyName);
    public VectorPath GetTextPath(IntPtr objectPointer, string text);
    public double MeasureText(IntPtr objectPointer, string text, out RectD bounds, Paint? paint = null);
    public int BreakText(IntPtr objectPointer, string text, double maxWidth, out float measuredWidth);
    public VecF[] GetGlyphPositions(IntPtr objectPointer, string text);
    public float[] GetGlyphWidths(IntPtr objectPointer, string text);
    public float[] GetGlyphWidths(IntPtr objectPointer, string text, Paint paint);
    public bool GetSubPixel(IntPtr objectPointer);
    public void SetSubPixel(IntPtr objectPointer, bool value);
    public FontEdging GetEdging(IntPtr objectPointer);
    public void SetEdging(IntPtr objectPointer, FontEdging value);
    public bool GetBold(IntPtr objectPointer);
    public void SetBold(IntPtr objectPointer, bool value, FontFamilyName family);
    public bool GetItalic(IntPtr objectPointer);
    public void SetItalic(IntPtr objectPointer, bool value, FontFamilyName family);
    public int GetGlyphCount(IntPtr objectPointer);
    public ushort[] GetGlyphs(IntPtr objectPointer, int[] codePoints);
    public bool ContainsGlyph(IntPtr objectPointer, int codePoint);
}
