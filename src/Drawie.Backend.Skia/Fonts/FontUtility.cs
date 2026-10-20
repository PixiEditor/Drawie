using System.Globalization;
using SkiaSharp;

namespace Drawie.Skia.Fonts;

public class FontUtility
{
    public readonly record struct FontRun(int Start, int Length, SKTypeface Typeface);

    public static IEnumerable<FontRun> GetFontRuns(string text, SKFont baseFont)
    {
        if (string.IsNullOrEmpty(text))
            yield break;

        var elements = StringInfo.GetTextElementEnumerator(text);
        var runs = new List<FontRun>();

        int runStart = 0;
        SKTypeface currentTypeface = baseFont.Typeface;

        while (elements.MoveNext())
        {
            string element = elements.GetTextElement();
            int elementStart = elements.ElementIndex;

            SKTypeface typeface = baseFont.ContainsGlyphs(element)
                ? baseFont.Typeface
                : GetFallbackTypeface(element, baseFont) ?? baseFont.Typeface;

            if (elementStart == 0)
            {
                currentTypeface = typeface;
                continue;
            }

            if (typeface != currentTypeface)
            {
                runs.Add(new FontRun(runStart, elementStart - runStart, currentTypeface));
                runStart = elementStart;
                currentTypeface = typeface;
            }
        }

        if (runStart < text.Length)
            runs.Add(new FontRun(runStart, text.Length - runStart, currentTypeface));

        foreach (var run in runs)
            yield return run;
    }

    private static SKTypeface? GetFallbackTypeface(string element, SKFont baseFont)
    {
        foreach (var rune in element.EnumerateRunes())
        {
            if (baseFont.ContainsGlyphs(rune.ToString()))
                continue;

            var fallback = SKFontManager.Default.MatchCharacter(
                baseFont.Typeface.FamilyName,
                baseFont.Typeface.FontWeight,
                baseFont.Typeface.FontWidth,
                baseFont.Typeface.FontSlant,
                null,
                rune.Value);

            if (fallback is null || !fallback.ContainsGlyphs(element))
                return null;

            return fallback;
        }

        return null;
    }

    public static SKFont CreateFont(SKFont baseFont, SKTypeface typeface)
    {
        return new SKFont(typeface, baseFont.Size)
        {
            Edging = baseFont.Edging,
            Hinting = baseFont.Hinting,
            BaselineSnap = baseFont.BaselineSnap,
            Embolden = baseFont.Embolden,
            EmbeddedBitmaps = baseFont.EmbeddedBitmaps,
            ForceAutoHinting = baseFont.ForceAutoHinting,
            LinearMetrics = baseFont.LinearMetrics,
            Subpixel = baseFont.Subpixel,
            ScaleX = baseFont.ScaleX,
            SkewX = baseFont.SkewX,
        };
    }
}
