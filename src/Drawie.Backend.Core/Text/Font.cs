using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Text;

public class Font : NativeObject
{
    public override object Native => DrawingBackendApi.Current.FontImplementation.GetNative(ObjectPointer);

    public double Size
    {
        get => DrawingBackendApi.Current.FontImplementation.GetFontSize(ObjectPointer);
        set
        {
            double oldSize = Size;
            DrawingBackendApi.Current.FontImplementation.SetFontSize(ObjectPointer, value);
            if (oldSize != value)
            {
                Changed?.Invoke();
            }
        }
    }

    public FontFamilyName Family { get; private set; }

    public bool SubPixel
    {
        get => DrawingBackendApi.Current.FontImplementation.GetSubPixel(ObjectPointer);
        set
        {
            bool wasSubPixel = SubPixel;
            DrawingBackendApi.Current.FontImplementation.SetSubPixel(ObjectPointer, value);
            if (wasSubPixel != value)
            {
                Changed?.Invoke();
            }
        }
    }

    public FontEdging Edging
    {
        get => DrawingBackendApi.Current.FontImplementation.GetEdging(ObjectPointer);
        set
        {
            FontEdging oldEdging = Edging;
            DrawingBackendApi.Current.FontImplementation.SetEdging(ObjectPointer, value);
            if (oldEdging != value)
            {
                Changed?.Invoke();
            }
        }
    }

    public bool IsDisposed { get; private set; }
    public int GlyphCount => DrawingBackendApi.Current.FontImplementation.GetGlyphCount(ObjectPointer);

    public bool Bold
    {
        get => DrawingBackendApi.Current.FontImplementation.GetBold(ObjectPointer);
        set
        {
            bool wasBold = Bold;
            DrawingBackendApi.Current.FontImplementation.SetBold(ObjectPointer, value, Family);
            if (wasBold != value)
            {
                Changed?.Invoke();
            }
        }
    }

    public bool Italic
    {
        get => DrawingBackendApi.Current.FontImplementation.GetItalic(ObjectPointer);
        set
        {
            bool wasItalic = Italic;
            DrawingBackendApi.Current.FontImplementation.SetItalic(ObjectPointer, value, Family);
            if (wasItalic != value)
            {
                Changed?.Invoke();
            }
        }
    }

    public event Action Changed;

    public Font(IntPtr objPtr, FontFamilyName family) : base(objPtr)
    {
        Family = family;
    }

    public override void Dispose()
    {
        if (IsDisposed) return;

        IsDisposed = true;
        DrawingBackendApi.Current.FontImplementation.Dispose(ObjectPointer);
    }

    public static Font? FromStream(Stream stream, float fontSize = 12f, float scaleX = 1f, float skewY = 0f)
    {
        return DrawingBackendApi.Current.FontImplementation.FromStream(stream, fontSize, scaleX, skewY);
    }

    public double MeasureText(string text)
    {
        return DrawingBackendApi.Current.FontImplementation.MeasureText(ObjectPointer, text);
    }

    public double MeasureText(string text, out RectD rectD, Paint? paint = null)
    {
        return DrawingBackendApi.Current.FontImplementation.MeasureText(ObjectPointer, text, out rectD, paint);
    }

    public int BreakText(string text, double maxWidth, out float measuredWidth)
    {
        return DrawingBackendApi.Current.FontImplementation.BreakText(ObjectPointer, text, maxWidth, out measuredWidth);
    }

    public VectorPath GetTextPath(string text)
    {
        return DrawingBackendApi.Current.FontImplementation.GetTextPath(ObjectPointer, text);
    }

    public static Font CreateDefault(float fontSize = 12f)
    {
        return DrawingBackendApi.Current.FontImplementation.CreateDefault(fontSize);
    }

    public static Font? FromFamilyName(string familyName)
    {
        return DrawingBackendApi.Current.FontImplementation.FromFamilyName(familyName);
    }

    public static Font? FromFontFamily(FontFamilyName familyName)
    {
        if (familyName.FontUri != null)
        {
            bool isFile = familyName.FontUri.IsFile;
            if (isFile)
            {
                if (!File.Exists(familyName.FontUri.LocalPath))
                {
                    return null;
                }

                using var stream = File.OpenRead(familyName.FontUri.LocalPath);
                var font = FromStream(stream);
                if (font != null)
                {
                    font.Family = familyName;
                }

                return font;
            }
        }

        return DrawingBackendApi.Current.FontImplementation.FromFamilyName(familyName.Name);
    }

    public VecF[] GetGlyphPositions(string text)
    {
        return DrawingBackendApi.Current.FontImplementation.GetGlyphPositions(ObjectPointer, text);
    }

    public float[] GetGlyphWidths(string text)
    {
        return DrawingBackendApi.Current.FontImplementation.GetGlyphWidths(ObjectPointer, text);
    }

    public float[] GetGlyphWidths(string text, Paint paint)
    {
        return DrawingBackendApi.Current.FontImplementation.GetGlyphWidths(ObjectPointer, text, paint);
    }

    public ushort[] GetGlyphs(int[] codePoints)
    {
        return DrawingBackendApi.Current.FontImplementation.GetGlyphs(ObjectPointer, codePoints);
    }

    public bool ContainsGlyph(int codePoint)
    {
        return DrawingBackendApi.Current.FontImplementation.ContainsGlyph(ObjectPointer, codePoint);
    }

    public override int GetHashCode()
    {
        return Family.GetHashCode();
    }

    protected bool Equals(Font other)
    {
        if (IsDisposed)
        {
            return ReferenceEquals(this, other);
        }

        return Family.Equals(other.Family) && Size.Equals(other.Size) && SubPixel == other.SubPixel &&
               Edging == other.Edging && Bold == other.Bold && Italic == other.Italic && GlyphCount == other.GlyphCount;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Font)obj);
    }
}
