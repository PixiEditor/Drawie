namespace Drawie.Backend.Core.Surfaces;

public interface IPixelsMap
{
    public Pixmap PeekPixels();
    public void MarkPixelsChanged();
}
