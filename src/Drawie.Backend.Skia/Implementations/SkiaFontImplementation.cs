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
    private readonly SkiaPathImplementation pathImplementation;

    private volatile int fontCounter = 0;

    public SkiaFontImplementation(SkiaPathImplementation pathImplementation)
    {
        this.pathImplementation = pathImplementation;
    }

    public object GetNative(IntPtr objectPointer)
    {
        TryGetInstance(objectPointer, out SKFont? font);
        return font;
    }

    public VectorPath GetTextPath(IntPtr objectPointer, string text)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            var path = font.GetTextPath(text);
            pathImplementation.AddManagedInstance(path);
            return new VectorPath(path.Handle);
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public Font? FromStream(Stream stream, float fontSize = 12f, float scaleX = 1f, float skewY = 0f)
    {
        SKTypeface typeface = SKTypeface.FromStream(stream);

        if(typeface == null)
        {
            return null;
        }

        SKFont font = new(typeface, fontSize, scaleX, skewY);
        int handle = Interlocked.Increment(ref fontCounter);
        AddManagedInstance(handle, font);
        return new Font(handle, new FontFamilyName(typeface.FamilyName));
    }

    public double GetFontSize(IntPtr objectPointer)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            return font.Size;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public void SetFontSize(IntPtr objectPointer, double value)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            font.Size = (float)value;
            return;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public double MeasureText(IntPtr objectPointer, string text)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            return font.MeasureText(text);
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public double MeasureText(IntPtr objectPointer, string text, out RectD bounds, Paint? paint = null)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
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
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            return font.BreakText(text, (float)maxWidth, out measuredWidth);
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public VecF[] GetGlyphPositions(IntPtr objectPointer, string text)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            SKPoint[] skPoints = font.GetGlyphPositions(text);
            return CastUtility.UnsafeArrayCast<SKPoint, VecF>(skPoints);
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public float[] GetGlyphWidths(IntPtr objectPointer, string text)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            float[] widths = font.GetGlyphWidths(text);
            return widths;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public float[] GetGlyphWidths(IntPtr objectPointer, string text, Paint paint)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            float[] widths = font.GetGlyphWidths(text, (SKPaint)paint.Native);
            return widths;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public bool GetSubPixel(IntPtr objectPointer)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            return font.Subpixel;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public void SetSubPixel(IntPtr objectPointer, bool value)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            font.Subpixel = value;
            return;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public FontEdging GetEdging(IntPtr objectPointer)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            return (FontEdging)font.Edging;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public void SetEdging(IntPtr objectPointer, FontEdging fontEdging)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            font.Edging = (SKFontEdging)fontEdging;
            return;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public bool GetBold(IntPtr objectPointer)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            return font.Typeface.IsBold || font.Embolden;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public void SetBold(IntPtr objectPointer, bool value, FontFamilyName family)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            if (family.FontUri is { IsFile: true })
            {
                font.Embolden = value;
            }
            else
            {
                if (font.Typeface.IsBold == value)
                {
                    return;
                }

                if (family.FontUri is { IsFile: true } && font.Embolden == value)
                {
                    return;
                }

                bool italic = GetItalic(objectPointer);
                UpdateTypeface(objectPointer, italic, value, font);
            }

            return;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public bool GetItalic(IntPtr objectPointer)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            return font.Typeface.IsItalic || font.SkewX != 0;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public void SetItalic(IntPtr objectPointer, bool value, FontFamilyName family)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            if (family.FontUri is { IsFile: true })
            {
                font.SkewX = value ? -0.25f : 0;
            }
            else
            {
                if (font.Typeface.IsItalic == value)
                {
                    return;
                }

                bool bold = GetBold(objectPointer);
                UpdateTypeface(objectPointer, value, bold, font);
            }

            return;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    private void UpdateTypeface(IntPtr objectPointer, bool italic, bool bold, SKFont font)
    {
        SKFontStyle fontStyle = SKFontStyle.Normal;
        if (bold && italic)
        {
            fontStyle = SKFontStyle.BoldItalic;
        }
        else if (bold)
        {
            fontStyle = SKFontStyle.Bold;
        }
        else if (italic)
        {
            fontStyle = SKFontStyle.Italic;
        }

        SKTypeface newTypeFace = SKTypeface.FromFamilyName(font.Typeface.FamilyName, fontStyle);

        SKFont newFont = new(newTypeFace, font.Size);
        newFont.Subpixel = font.Subpixel;
        newFont.Edging = font.Edging;
        newFont.Embolden = font.Embolden;
        newFont.SkewX = font.SkewX;

        UpdateManagedInstance(objectPointer, newFont);
    }

    public int GetGlyphCount(IntPtr objectPointer)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            return font.Typeface.GlyphCount;
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public ushort[] GetGlyphs(IntPtr objectPointer, int[] codePoints)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            return font.GetGlyphs(codePoints);
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public bool ContainsGlyph(IntPtr objectPointer, int glyphId)
    {
        if (TryGetInstance(objectPointer, out SKFont? font))
        {
            return font.ContainsGlyph(glyphId);
        }

        throw new InvalidOperationException("Native font object not found");
    }

    public Font CreateDefault(float fontSize)
    {
        SKFont font = new(SKTypeface.Default, fontSize);
        int handle = Interlocked.Increment(ref fontCounter);
        AddManagedInstance(handle, font);
        return new Font(handle, new FontFamilyName(SKTypeface.Default.FamilyName));
    }

    public Font? FromFamilyName(string familyName)
    {
        SKTypeface typeface = SKTypeface.FromFamilyName(familyName);
        if (typeface == null)
        {
            return null;
        }

        SKFont font = new(typeface);
        int handle = Interlocked.Increment(ref fontCounter);
        AddManagedInstance(handle, font);
        return new Font(handle, new FontFamilyName(familyName));
    }

    public Font? FromFamilyName(string familyName, FontStyleWeight weight, FontStyleWidth width, FontStyleSlant slant)
    {
        SKTypeface typeface = SKTypeface.FromFamilyName(familyName, (SKFontStyleWeight)weight, (SKFontStyleWidth)width,
            (SKFontStyleSlant)slant);
        if (typeface == null)
        {
            return null;
        }

        SKFont font = new(typeface);
        int handle = Interlocked.Increment(ref fontCounter);
        AddManagedInstance(handle, font);
        return new Font(handle, new FontFamilyName(familyName));
    }

    public void Dispose(IntPtr objectPointer)
    {
        UnmanageAndDispose(objectPointer);
    }
}
