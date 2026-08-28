using Drawie.ShaderCompiler.Compilation;

string code = """
              struct VertexInput
              {
                  float3 vPos       : POSITION;
                  float3 vNormal    : NORMAL;
                  float2 vTexCoords : TEXCOORD0;
              };

              struct VertexOutput
              {
                  float4 position   : SV_Position;
                  float3 fNormal    : TEXCOORD0;
                  float3 fPos       : TEXCOORD1;
                  float2 fTexCoords : TEXCOORD2;
              };

              [[vk::binding(0, 0)]]
              cbuffer Transform
              {
                  float4x4 uModel;
                  float4x4 uView;
                  float4x4 uProjection;
              };

              [shader("vertex")]
              VertexOutput VSMain(VertexInput input)
              {
                  VertexOutput output;

                  output.position = mul(
                      mul(
                          mul(uProjection, uView),
                          uModel
                      ),
                      float4(input.vPos, 1.0)
                  );

                  output.fPos = mul(
                      uModel,
                      float4(input.vPos, 1.0)
                  ).xyz;

                  float3x3 model3x3 = float3x3(uModel);

                 output.fNormal = mul(
                     float3x3(uModel),
                     input.vNormal
                 );

                  output.fTexCoords = input.vTexCoords;

                  return output;
              }
              """;

ShaderCompiler compiler = new ShaderCompiler("", "shader.slang");
compiler.Compile(code, CompilationTarget.GlslEs3);