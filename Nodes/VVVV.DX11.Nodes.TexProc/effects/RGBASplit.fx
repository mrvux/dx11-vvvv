Texture2D InputTexture : TEXTURE;

void VS(float4 posObject : POSITION, out float4 posScreen : SV_Position)
{
	posScreen = posObject;
}

void PS_Float(float4 pixelPos : SV_POSITION, 
out float red : SV_Target0,
out float green : SV_Target1, 
out float blue : SV_Target2,
out float alpha : SV_Target3)
{
	float4 c = InputTexture.Load(int3(pixelPos.xy, 0));
	red = c.r;
	green = c.g;
	blue = c.b;
	alpha = c.a;
}

void PS_Int(float4 pixelPos : SV_POSITION, 
out int red : SV_Target0,
out int green : SV_Target1, 
out int blue : SV_Target2,
out int alpha : SV_Target3)
{
	int4 c = InputTexture.Load(int3(pixelPos.xy, 0));
	red = c.r;
	green = c.g;
	blue = c.b;
	alpha = c.a;
}

void PS_UInt(float4 pixelPos : SV_POSITION, 
out uint red : SV_Target0,
out uint green : SV_Target1, 
out uint blue : SV_Target2,
out uint alpha : SV_Target3)
{
	uint4 c = InputTexture.Load(int3(pixelPos.xy, 0));
	red = c.r;
	green = c.g;
	blue = c.b;
	alpha = c.a;
}

//Float, used for floating point texture as well as unorm
technique11 ApplyFloat
{
	pass P0
	{
		SetVertexShader (CompileShader(vs_4_0,VS()));
		SetPixelShader (CompileShader(ps_4_0,PS_Float()));
	}
}

//int typed textures
technique11 ApplyInt
{
	pass P0
	{
		SetVertexShader (CompileShader(vs_4_0,VS()));
		SetPixelShader (CompileShader(ps_4_0,PS_Int()));
	}
}

//uint types textures
technique11 ApplyUInt
{
	pass P0
	{
		SetVertexShader (CompileShader(vs_4_0,VS()));
		SetPixelShader (CompileShader(ps_4_0,PS_UInt()));
	}
}
