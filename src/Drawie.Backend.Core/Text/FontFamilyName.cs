using System.Diagnostics.CodeAnalysis;

namespace Drawie.Backend.Core.Text;

public struct FontFamilyName
{
    public string Name { get; set; }
    public Uri? FontUri { get; set; }

    public FontFamilyName(string name)
    {
        Name = name;
    }

    public FontFamilyName(Uri fontUri, string name)
    {
        Name = name;
        FontUri = fontUri;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is FontFamilyName fontFamilyName)
        {
            return Name == fontFamilyName.Name && FontUri == fontFamilyName.FontUri;
        }

        return base.Equals(obj);
    }
}
