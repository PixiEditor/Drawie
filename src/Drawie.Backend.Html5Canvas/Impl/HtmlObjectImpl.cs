using System.Collections.Concurrent;

namespace Drawie.Html5Canvas.Impl;

public class HtmlObjectImpl<T>
{
    public ConcurrentDictionary<int, T> ManagedObjects { get; } = new ConcurrentDictionary<int, T>();
}