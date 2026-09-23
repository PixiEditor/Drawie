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

        if (baseFont.ContainsGlyphs(text))
        {
            yield return new FontRun(0, text.Length, baseFont.Typeface);
            yield break;
        }

        var elements = StringInfo.GetTextElementEnumerator(text);

        int start = 0;
        SKTypeface currentTypeface = baseFont.Typeface;

        while (elements.MoveNext())
        {
            string element = elements.GetTextElement();

            if (currentTypeface.ContainsGlyphs(element))
                continue;

            int elementStart = elements.ElementIndex;

            if (elementStart > start)
            {
                yield return new FontRun(
                    start,
                    elementStart - start,
                    currentTypeface);
            }

            int runStart = elementStart;
            int runEnd = elementStart;

            bool setFallback = false;
            do
            {
                element = elements.GetTextElement();
                runEnd = elements.ElementIndex + element.Length;

                if (currentTypeface.ContainsGlyphs(element))
                    break;

                int codepoint = element.EnumerateRunes().First().Value;

                SKTypeface? fallback =
                    SKFontManager.Default.MatchCharacter(baseFont.Typeface.FamilyName, baseFont.Typeface.FontWeight,
                        baseFont.Typeface.FontWidth, baseFont.Typeface.FontSlant, null,
                        codepoint);

                if (fallback != null)
                {
                    currentTypeface = fallback;
                    if(setFallback)
                        break;

                    setFallback = true;
                }
            } while (elements.MoveNext());

            if (runEnd > runStart)
            {
                yield return new FontRun(
                    runStart,
                    runEnd - runStart,
                    currentTypeface);
            }

            start = runEnd;
            currentTypeface = baseFont.Typeface;

            if (runEnd >= text.Length)
                break;
        }

        if (start < text.Length)
        {
            yield return new FontRun(start, text.Length - start, baseFont.Typeface);
        }
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
