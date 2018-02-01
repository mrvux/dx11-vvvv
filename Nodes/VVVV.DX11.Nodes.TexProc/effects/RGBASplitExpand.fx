Texture2D InputTexture : TEXTURE;

void VS(float4 posObject : POSITION, out float4 posScreen : SV_Position)
{
	posScreen = posObject;
}

void PS_Float(float4 pixelPos : SV_POSITION, 
out float4 red : SV_Target0,
out float4 green : SV_Target1,
out float4 blue : SV_Target2,
out float4 alpha : SV_Target3)
{
	float4 c = InputTexture.Load(int3(pixelPos.xy, 0));
	red = c.rrrr;
	green = c.gggg;
	blue = c.bbbb;
	alpha = c.aaaa;
}

void PS_Int(float4 pixelPos : SV_POSITION, 
out int4 red : SV_Target0,
out int4 green : SV_Target1,
out int4 blue : SV_Target2,
out int4 alpha : SV_Target3)
{
	int4 c = InputTexture.Load(int3(pixelPos.xy, 0));
    red = c.rrrr;
    green = c.gggg;
    blue = c.bbbb;
    alpha = c.aaaa;
}

void PS_UInt(float4 pixelPos : SV_POSITION, 
out uint4 red : SV_Target0,
out uint4 green : SV_Target1,
out uint4 blue : SV_Target2,
out uint4 alpha : SV_Target3)
{
	uint4 c = InputTexture.Load(int3(pixelPos.xy, 0));
    red = c.rrrr;
    green = c.gggg;
    blue = c.bbbb;
    alpha = c.aaaa;
}

//Float, used for floating point texture as well as unorm
technique11 ApplyFloat
{
	pass P0
	{
		SetPixelShader (CompileShader(ps_4_0,PS_Float()));
	}
}

//int typed textures
technique11 ApplyInt
{
	pass P0
	{
		SetPixelShader (CompileShader(ps_4_0,PS_Int()));
	}
}

//uint types textures
technique11 ApplyUInt
{
	pass P0
	{
		SetPixelShader (CompileShader(ps_4_0,PS_UInt()));
	}
}
