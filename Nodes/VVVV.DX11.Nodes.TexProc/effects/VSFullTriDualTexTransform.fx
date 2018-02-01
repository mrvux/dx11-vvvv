struct vs2ps
{
    float4 position: SV_POSITION;
    float2 uv1 : TEXCOORD0;
    float2 uv2 : TEXCOORD1;
};

cbuffer cbTexTransform : register(b0)
{
    float4x4 tTex1;
    float4x4 tTex2;
}


vs2ps VS_Both( uint vertexID : SV_VertexID )
{
    vs2ps result;

    float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
    result.position = float4(uv * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f), 0.0f, 1.0f);
    result.uv1 = mul(float4(uv, 0.0f, 1.0f), tTex1).xy;
    result.uv2 = mul(float4(uv, 0.0f, 1.0f), tTex2).xy;
    
	return result;
}

vs2ps VS_Second(uint vertexID : SV_VertexID)
{
    vs2ps result;

    float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
    result.position = float4(uv * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f), 0.0f, 1.0f);
    result.uv1 = uv;
    result.uv2 = mul(float4(uv, 0.0f, 1.0f), tTex2).xy;

    return result;
}

technique10 ApplySecondOnly
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_4_0, VS_Second()));
    }
}


technique10 ApplyBoth
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS_Both() ) );
	}
}
