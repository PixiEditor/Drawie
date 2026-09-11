using Drawie.ShaderCompiler.Compilation;

string code = """
              struct VSOutput
              {
                  float4 position : SV_Position;
                  float2 normPosition : TEXCOORD0;
                  float4 color : TEXCOORD1;
                  nointerpolation float2 antiAliasing : TEXCOORD2; // x component is 0 - disabled, 1 enabled
                  uint textureIndex : TEXCOORD3;
              };
              
              [[vk::binding(0, 1)]]
              Texture2D<float4> textures[];
              
              [[vk::binding(1, 1)]]
              SamplerState sampler;
              
              [shader("fragment")]
              float4 fragmentMain(VSOutput input) : SV_Target
              {
                  return textures[input.textureIndex].Sample(sampler, input.normPosition);
              }
              """;

ShaderCompiler compiler = new ShaderCompiler("", "shader.slang");
compiler.ModulesPath = "";
compiler.Compile(code, CompilationTarget.SpirV);