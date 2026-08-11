using Drawie.Backend.Vertie.Rendering;

string code = """
              [__AttributeUsage(_AttributeTargets.Var)]
              struct DragFloatAttribute
              {
                  string Description;
              
                  float MinValue;
              
                  float MaxValue;
              
                  float Speed;
              };
              
              struct Constants
              {
                  float4x4 Projection;
              
                  float4x4 View;
              
                  float4x4 Model;
              };
              
              struct VSInput
              {
                  float2 Position : POSITION0;
              
                  float2 UV : TEXCOORD0;
              
                  float4 Color : COLOR0;
              };
              
              struct VSOutput
              {
                  float4 Position : SV_POSITION;
              
                  float2 UV : TEXCOORD0;
              
                  float4 Color : COLOR0;
              };
              
              struct Block
              {
                  Constants Constants;
              
                  Texture2D Texture;
              
                  SamplerState Sampler;
              
                  RWTexture2D<float4> OutputTexture;
              };
              
              uniform ParameterBlock<Block> block;
              
              [DragFloat("Gamma", 0.0, 3.0, 0.1)]
              uniform float Gamma;
              
              float3 SrgbToLinear(float3 srgb)
              {
                  return srgb * (srgb * (srgb * 0.305306011 + 0.682171111) + 0.012522878);
              }
              
              [shader("vertex")]
              VSOutput VSMain(VSInput input)
              {
                  float4x4 mvp = block.Constants.Projection * block.Constants.View * block.Constants.Model;
              
                  VSOutput output;
              
                  output.Position = mul(mvp, float4(input.Position, 0.0f, 1.0f));
                  output.UV = input.UV;
                  output.Color = input.Color;
                  output.Color.rgb = SrgbToLinear(output.Color.rgb);
              
                  return output;
              }
              
              [shader("pixel")]
              float4 PSMain(VSOutput input) : SV_TARGET
              {
                  return input.Color * block.Texture.Sample(block.Sampler, input.UV);
              }
              
              [shader("compute")]
              [numthreads(8, 8, 1)]
              void CSMain(uint3 DTid: SV_DispatchThreadID)
              {
                  block.OutputTexture[DTid.xy] = float4(1.0f, 0.0f, 0.0f, 1.0f);
              }
              """;

ShaderDefinition shaderDefinition = new ShaderDefinition(code);
shaderDefinition.Compile();