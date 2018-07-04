Texture2D InputTexture : INPUTTEXTURE;

cbuffer cbPerDraw : register(b0)
{
	float4 Color : COLOR = 1.0f;
	float4x4 tW : WORLD;
	float4x4 tTex : TEXTUREMATRIX;
};

cbuffer cbPerBatch : register(b1)
{
	float4x4 tVP : VIEWPROJECTION;
	int WorldCount : WORLDCOUNT;
	int ColorCount : COLORCOUNT;
	int TextureMatrixCount : TEXTUREMATRIXCOUNT;
};

cbuffer cbPerLayer : register(b2)
{
    float layerOpacity : LAYEROPACITY;
};

StructuredBuffer<float4x4> WorldBuffer : WORLDBUFFER;
StructuredBuffer<float4> ColorBuffer : COLORBUFFER;
StructuredBuffer<float4x4> TextureMatrixBuffer : TEXTUREMATRIXBUFFER;

SamplerState linearSampler : SAMPLERSTATE
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

struct vsIn
{
	float4 pos : POSITION;
};

struct vsInTextured
{
	float4 pos : POSITION;
	float4 uv : TEXCOORD0;
};

struct vsInInstanced
{
	float4 pos : POSITION;
	uint ii : SV_InstanceID;
};

struct vsInInstancedTex
{
	float4 pos : POSITION;
	float4 uv : TEXCOORD0;
	uint ii : SV_InstanceID;
};

struct psIn
{
    float4 pos : SV_POSITION;
}; 

struct psInTextured
{
    float4 pos: SV_POSITION;
	float2 uv : TEXCOORD0;
}; 

struct psInColor
{
    float4 pos : SV_POSITION;
	float4 color : TEXCOORD0;
}; 

struct psInColorTextured
{
    float4 pos: SV_POSITION;
	float4 color : TEXCOORD0;
	float2 uv : TEXCOORD1;
}; 

psIn VS(vsIn input)
{
	psIn output;

	float4x4 wvp = mul(tW,tVP);
	output.pos =  mul(input.pos,wvp);
    return output;
}

psInTextured VS_Textured(vsInTextured input)
{
    psInTextured output;
	
	float4x4 wvp = mul(tW,tVP);
	output.pos =  mul(input.pos,wvp);

	float4 uv = input.uv;
	uv.xy -= 0.5f;
	uv.y *= -1.0f;
	uv = mul(uv,tTex);
	uv.y *= -1.0f;
	uv.xy += 0.5f;

	output.uv  = uv.xy;
    return output;
}

psInColor VS_Instanced(vsInInstancedTex input)
{
	psInColor output;

	float4x4 w = transpose(WorldBuffer[input.ii % WorldCount]);
	float4x4 wvp = mul(w,tVP);

	output.pos =  mul(input.pos,wvp);
	output.color = ColorBuffer[input.ii % ColorCount];
    return output;
}

psInColorTextured VS_Instanced_Textured(vsInInstancedTex input)
{
    psInColorTextured output;
	
	float4x4 w = transpose(WorldBuffer[input.ii % WorldCount]);
	float4x4 wvp = mul(w,tVP);

	float4x4 tt = transpose(TextureMatrixBuffer[input.ii % TextureMatrixCount]);

	output.pos =  mul(input.pos,wvp);
	output.color = ColorBuffer[input.ii % ColorCount];

	float4 uv = input.uv;
	uv.xy -= 0.5f;
	uv.y *= -1.0f;
	uv = mul(uv,tt);
	uv.y *= -1.0f;
	uv.xy += 0.5f;

	output.uv  = uv.xy;

    return output;
}

float4 PS(psIn input): SV_Target
{
	return Color;
}

float4 PS_Textured(psInTextured input): SV_Target
{
    return InputTexture.Sample(linearSampler,input.uv) * Color;
}

float4 PS_Color(psInColor input): SV_Target
{
	float4 c = input.color;
    c.a *= layerOpacity;
    return c;
}

float4 PS_Color_Textured(psInColorTextured input): SV_Target
{
    float4 c = InputTexture.Sample(linearSampler,input.uv) * input.color;
    c.a *= layerOpacity;
    return c;
}

technique10 Render
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}

technique10 RenderTextured
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Textured() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Textured() ) );
	}
}

technique10 RenderInstanced
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Instanced() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Color() ) );
	}
}

technique10 RenderInstancedTextured
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Instanced_Textured() ) );
		SetPixelShader( CompileShader( ps_4_0, PS_Color_Textured() ) );
	}
}