using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Texture = SilkNet.Rendering.Texture;

namespace SilkNet;

public class Gif : IDisposable
{
    public int FrameCount => _frames.Length;
    
    private Image<Rgba32> _gifImage;
    private Texture[] _frames;
    private GL _gl;
    
    public Gif(GL gl, string path)
    {
        _gifImage = Image.Load<Rgba32>(path);
        _gl = gl;
        LoadFrames();
    }

    public Texture GetFrame(int frame)
    {
        return _frames[frame];
    }
    
    private void LoadFrames()
    {
        _frames = new Texture[_gifImage.Frames.Count];
        for (int i = 0; i < _gifImage.Frames.Count; i++)
        {
            _frames[i] = new Texture(_gl, GetFrameData(i), (uint)_gifImage.Width, (uint)_gifImage.Height);
        }
    }
    
    private Span<byte> GetFrameData(int frame)
    {
        var memoryGroup = _gifImage.Frames[frame].PixelBuffer.MemoryGroup;
        return MemoryMarshal.AsBytes(memoryGroup[0].Span);
    }

    public void Dispose()
    {
        foreach (var frame in _frames)
        {
            frame.Dispose();
        }
    }
}