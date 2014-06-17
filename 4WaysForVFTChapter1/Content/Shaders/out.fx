sampler BloomSampler : register(s0);
texture ColorMap;

sampler ColorMapSampler = sampler_state

{
   Texture = <ColorMap>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;  
   AddressU  = Clamp;
   AddressV  = Clamp;
};

float BloomIntensity = 1.3;
float OriginalIntensity = 1.0;
float BloomSaturation = 1.0;
float OriginalSaturation = 1.0;

float4 AdjustSaturation(float4 color, float saturation)
{
    float grey = dot(color, float3(0.3, 0.59, 0.11));
    return lerp(grey, color, saturation);
}

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
      float4 bloomColor = tex2D(BloomSampler, texCoord);
      float4 originalColor = tex2D(ColorMapSampler, texCoord);
      bloomColor = AdjustSaturation(bloomColor, BloomSaturation) * BloomIntensity;
      originalColor = AdjustSaturation(originalColor, OriginalSaturation) * OriginalIntensity;
      return originalColor + bloomColor;
}

technique BloomCombine
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}