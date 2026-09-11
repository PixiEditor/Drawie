struct VSOutput
{
    float4 position : SV_Position;
    [vk::location(0)] float2 normPosition : TEXCOORD0;
    [vk::location(1)] float4 color : TEXCOORD1;
    [vk::location(2)] nointerpolation float2 antiAliasing : TEXCOORD2; // x component is 0 - disabled, 1 enabled
    [vk::location(3)] nointerpolation uint textureIndex : TEXCOORD3;
};
