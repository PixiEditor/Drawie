namespace Drawie.RenderApi.WebGpu.Extensions;

public static class StringExtensions
{
    public static unsafe char* ToPointer(this string text)
    {
        return (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(text);
    }

    public static unsafe string GetString(char* stringStart)
    {
        int characters = 0;
        while (stringStart[characters] != 0)
        {
            characters++;
        }

        return System.Text.Encoding.UTF8.GetString((byte*)stringStart, characters * 2);
    }
}