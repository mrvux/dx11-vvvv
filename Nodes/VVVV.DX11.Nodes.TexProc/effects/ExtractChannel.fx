Texture2D InputTexture : TEXTURE;

void VS(float4 posObject : POSITION, out float4 posScreen : SV_Position)
{
	posScreen = posObject;
}

void PS_FloatRed(float4 pixelPos : SV_POSITION, out float output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).r;
}

void PS_FloatGreen(float4 pixelPos : SV_POSITION, out float output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).g;
}

void PS_FloatBlue(float4 pixelPos : SV_POSITION, out float output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).b;
}

void PS_FloatAlpha(float4 pixelPos : SV_POSITION, out float output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).a;
}

void PS_IntRed(float4 pixelPos : SV_POSITION, out int output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).r;
}

void PS_IntGreen(float4 pixelPos : SV_POSITION, out int output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).g;
}

void PS_IntBlue(float4 pixelPos : SV_POSITION, out int output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).b;
}

void PS_IntAlpha(float4 pixelPos : SV_POSITION, out int output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).a;
}

void PS_UIntRed(float4 pixelPos : SV_POSITION, out uint output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).r;
}

void PS_UIntGreen(float4 pixelPos : SV_POSITION, out uint output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).g;
}

void PS_UIntBlue(float4 pixelPos : SV_POSITION, out uint output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).b;
}

void PS_UIntAlpha(float4 pixelPos : SV_POSITION, out uint output : SV_Target0)
{
    output = InputTexture.Load(int3(pixelPos.xy, 0)).a;
}
