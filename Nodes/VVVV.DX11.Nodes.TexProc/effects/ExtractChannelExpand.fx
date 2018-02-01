Texture2D InputTexture : TEXTURE;


void PS_FloatRed(float4 pixelPos : SV_POSITION, out float4 output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).rrrr;
}

void PS_FloatGreen(float4 pixelPos : SV_POSITION, out float4 output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).gggg;
}

void PS_FloatBlue(float4 pixelPos : SV_POSITION, out float4 output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).bbbb;
}

void PS_FloatAlpha(float4 pixelPos : SV_POSITION, out float4 output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).aaaa;
}

void PS_IntRed(float4 pixelPos : SV_POSITION, out int4 output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).rrrr;
}

void PS_IntGreen(float4 pixelPos : SV_POSITION, out int4 output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).gggg;
}

void PS_IntBlue(float4 pixelPos : SV_POSITION, out int4 output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).bbbb;
}

void PS_IntAlpha(float4 pixelPos : SV_POSITION, out int4 output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).aaaa;
}

void PS_UIntRed(float4 pixelPos : SV_POSITION, out uint4 output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).rrrr;
}

void PS_UIntGreen(float4 pixelPos : SV_POSITION, out uint4 output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).gggg;
}

void PS_UIntBlue(float4 pixelPos : SV_POSITION, out uint4 output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).bbbb;
}

void PS_UIntAlpha(float4 pixelPos : SV_POSITION, out uint4 output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).aaaa;
}

technique11 FloatRed
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_FloatRed()));
    }
}

technique11 FloatGreen
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_FloatGreen()));
    }
}

technique11 FloatBlue
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_FloatBlue()));
    }
}

technique11 FloatAlpha
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_FloatAlpha()));
    }
}

technique11 IntRed
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_IntRed()));
    }
}

technique11 IntGreen
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_IntGreen()));
    }
}

technique11 IntBlue
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_IntBlue()));
    }
}

technique11 IntAlpha
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_IntAlpha()));
    }
}

technique11 UIntRed
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_UIntRed()));
    }
}

technique11 UIntGreen
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_UIntGreen()));
    }
}

technique11 UIntBlue
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_UIntBlue()));
    }
}

technique11 UIntAlpha
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_UIntAlpha()));
    }
}