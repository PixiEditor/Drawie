struct VSOutput
{
    float4 position : SV_Position;
    float2 normPosition : TEXCOORD0;
    float4 color : TEXCOORD1;
    nointerpolation float2 antiAliasing : TEXCOORD2; // x component is 0 - disabled, 1 enabled
};
