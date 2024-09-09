namespace Drawie.RenderApi.WebGpu.Extensions;

public static class StringExtensions
{
    public static unsafe byte* ToPointer(this string str)
    {
        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
    
        fixed (byte* p = bytes)
        {
            return p;
        }
    }
}