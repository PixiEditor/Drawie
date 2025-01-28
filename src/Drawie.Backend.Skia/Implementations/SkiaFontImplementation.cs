using System.Collections.Concurrent;
using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
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
        return new Font(font.Handle);
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

    public Font CreateDefault(float fontSize)
    {
        SKFont font = new(SKTypeface.Default, fontSize);
        ManagedInstances[font.Handle] = font;
        return new Font(font.Handle);
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
        return new Font(font.Handle);
    }

    public void Dispose(IntPtr objectPointer)
    {
        if (ManagedInstances.TryRemove(objectPointer, out SKFont font))
        {
            font.Dispose();
        }
    }
}
