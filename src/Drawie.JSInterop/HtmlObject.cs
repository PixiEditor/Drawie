namespace Drawie.JSInterop;

public class HtmlObject
{
    public string TagName { get; set; } = string.Empty;
    public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();
    public int Id { get; internal set; }

    public void SetAttribute(string name, string value)
    {
        Attributes[name] = value;
        JSRuntime.InvokeJs($"document.getElementById('element{Id}').setAttribute('{name}', '{value}')");
    }
}