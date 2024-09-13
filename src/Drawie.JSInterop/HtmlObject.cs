namespace Drawie.JSInterop;

public class HtmlObject(string tagName)
{
    public string TagName { get; } = tagName; 
    public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();
    public string Id { get; internal set; }

    public void SetAttribute(string name, string value)
    {
        Attributes[name] = value;
        JSRuntime.InvokeJs($"document.getElementById('{Id}').setAttribute('{name}', '{value}')");
    }
}