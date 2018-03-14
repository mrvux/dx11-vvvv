Texture2D InputTexture : INPUTTEXTURE;

cbuffer cbTexTransform : register(b0)
{
	float4x4 tTex : TEXTUREMATRIX;
};

cbuffer cbColor : register(b1)
{
    float4 Color : COLOR;
};

SamplerState textureSampler : TEXTURESAMPLER
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

void VS(uint vertexID : SV_VertexID, out float4 posScreen : SV_Position, out float2 uv : TEXCOORD0)
{
    uv = float2((vertexID << 1) & 2, vertexID & 2);
    posScreen = float4(uv * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f), 0.0f, 1.0f);

    uv.xy -= 0.5f;
    uv.y *= -1.0f;
    uv = mul(float4(uv, 0.0f, 1.0f), tTex).xy;
    uv.y *= -1.0f;
    uv.xy += 0.5f;
}

float4 PS(float4 posScreen : SV_Position, float2 uv : TEXCOORD0): SV_Target
{
    return InputTexture.Sample(textureSampler,uv) * Color;
}

technique11 Render
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}
