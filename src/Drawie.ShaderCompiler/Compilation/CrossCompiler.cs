using System.Reflection;
using System.Runtime.InteropServices;
using Drawie.ShaderCompiler;
using Silk.NET.SPIRV.Cross;

namespace Drawie.Backend.Vertie.Compilation;

public class CrossCompiler
{
    private Cross api;
    private SpvContext spvContext;
    
    public unsafe CrossCompiler()
    {
        LoadNativeLibs();
        api = Cross.GetApi();
        spvContext = new SpvContext(api);
    }

    private void LoadNativeLibs()
    {
        var location = Path.GetDirectoryName(typeof(ShaderCompilerTask).Assembly.Location);
        string pathToNative = Path.Combine(location, "runtimes", RuntimeInformation.RuntimeIdentifier, "native");
        string pathToLib = Path.Combine(pathToNative, GetLibName());
        
        NativeLibrary.Load(pathToLib);
    }

    private string GetLibName()
    {
        if (RuntimeInformation.RuntimeIdentifier.StartsWith("win"))
        {
            return "spirv-cross.dll";
        }

        if (RuntimeInformation.RuntimeIdentifier.StartsWith("linux"))
        {
            return "libspirv-cross.so";
        }

        if (RuntimeInformation.RuntimeIdentifier.StartsWith("osx"))
        {
            return "libspirv-cross.dylib";
        }
        throw new PlatformNotSupportedException();
    }

    public string CompileToGlslEs3(byte[] spirv)
    {
        var compiler = spvContext.CreateCompiler(Silk.NET.SPIRV.Cross.Backend.Glsl, spirv);
        compiler.ConfigureGlslVersion(GlslVersion.GlslEs300);
        return compiler.Compile();
    }
}