Texture2D InputTexture : TEXTURE;

SamplerState linearSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct vsInput
{
	float4 Pos : POSITION;
	float2 TexCd : TEXCOORD0;
};

struct psInput
{
    float4 Pos: SV_POSITION;
    float2 TexCd: TEXCOORD0;
};

psInput VS(vsInput input)
{
	psInput output;
    output.Pos  = input.Pos;
    output.TexCd = input.TexCd;
    return output;
}

float4 PS(psInput input): SV_Target
{
    return InputTexture.Sample(linearSampler, input.TexCd);
}

technique11 Render
{
	pass P0
	{
		SetVertexShader (CompileShader(vs_4_0,VS()));
		SetPixelShader (CompileShader(ps_4_0,PS()));
	}
}

