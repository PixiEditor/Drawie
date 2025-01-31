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
}
