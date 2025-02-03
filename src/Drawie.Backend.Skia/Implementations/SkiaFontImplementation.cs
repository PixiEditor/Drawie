using System.Collections.Concurrent;
using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations;

public class SkiaFontImplementation : SkObjectImplementation<SKFont>, IFontImplementation
{
    private readonly ConcurrentDictionary<IntPtr, SKTypeface> ManagedTypefaces = new();

    private readonly SkiaPathImplementation pathImplementation;

    public SkiaFontImplementation(SkiaPathImplementation pathImplementation)
    {
        this.pathImplementation = pathImplementation;
    }

    public object GetNative(IntPtr objectPointer)
    {
        ManagedInstances.TryGetValue(objectPointer, out SKFont? font);
        return font;
    }

    public VectorPath GetTextPath(IntPtr objectPointer, string text)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            var path = font.GetTextPath(text);
            pathImplementation.ManagedInstances[path.Handle] = path;
            return new VectorPath(path.Handle);
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public Font FromStream(Stream stream, float fontSize = 12f, float scaleX = 1f, float skewY = 0f)
    {
        SKTypeface typeface = SKTypeface.FromStream(stream);
        ManagedTypefaces[typeface.Handle] = typeface;

        SKFont font = new(typeface, fontSize, scaleX, skewY);
        ManagedInstances[font.Handle] = font;
        return new Font(font.Handle) { Family = new FontFamilyName(typeface.FamilyName) };
    }

    public double GetFontSize(IntPtr objectPointer)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            return font.Size;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public void SetFontSize(IntPtr objectPointer, double value)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            font.Size = (float)value;
            return;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public double MeasureText(IntPtr objectPointer, string text)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            return font.MeasureText(text);
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public double MeasureText(IntPtr objectPointer, string text, out RectD bounds, Paint? paint = null)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            SKPaint? skPaint = (SKPaint)paint?.Native;
            double measurement = font.MeasureText(text, out SKRect skBounds, skPaint);
            bounds = new RectD(skBounds.Left, skBounds.Top, skBounds.Width, skBounds.Height);
            return measurement;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public int BreakText(IntPtr objectPointer, string text, double maxWidth, out float measuredWidth)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            return font.BreakText(text, (float)maxWidth, out measuredWidth);
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public VecF[] GetGlyphPositions(IntPtr objectPointer, string text)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            SKPoint[] skPoints = font.GetGlyphPositions(text);
            return CastUtility.UnsafeArrayCast<SKPoint, VecF>(skPoints);
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public float[] GetGlyphWidths(IntPtr objectPointer, string text)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            float[] widths = font.GetGlyphWidths(text);
            return widths;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public float[] GetGlyphWidths(IntPtr objectPointer, string text, Paint paint)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            float[] widths = font.GetGlyphWidths(text, (SKPaint)paint.Native);
            return widths;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public bool GetSubPixel(IntPtr objectPointer)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            return font.Subpixel;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public void SetSubPixel(IntPtr objectPointer, bool value)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            font.Subpixel = value;
            return;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public FontEdging GetEdging(IntPtr objectPointer)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            return (FontEdging)font.Edging;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public void SetEdging(IntPtr objectPointer, FontEdging fontEdging)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            font.Edging = (SKFontEdging)fontEdging;
            return;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public int GetGlyphCount(IntPtr objectPointer)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            return font.Typeface.GlyphCount;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public ushort[] GetGlyphs(IntPtr objectPointer, int[] codePoints)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            return font.GetGlyphs(codePoints);
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public bool ContainsGlyph(IntPtr objectPointer, int glyphId)
    {
        if (ManagedInstances.TryGetValue(objectPointer, out SKFont? font))
        {
            return font.ContainsGlyph(glyphId);
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public Font CreateDefault(float fontSize)
    {
        SKFont font = new(SKTypeface.Default, fontSize);
        ManagedInstances[font.Handle] = font;
        return new Font(font.Handle) { Family = new FontFamilyName(SKTypeface.Default.FamilyName) };
    }

    public Font? FromFamilyName(string familyName)
    {
        SKTypeface typeface = SKTypeface.FromFamilyName(familyName);
        if (typeface == null)
        {
            return null;
        }

        ManagedTypefaces[typeface.Handle] = typeface;

        SKFont font = new(typeface);
        ManagedInstances[font.Handle] = font;
        return new Font(font.Handle) { Family = new FontFamilyName(familyName) };
    }

    public void Dispose(IntPtr objectPointer)
    {
        if (ManagedInstances.TryRemove(objectPointer, out SKFont font))
        {
            font.Dispose();
        }
    }
}
