StructuredBuffer<int2> uvBuffer;
StructuredBuffer<int> uvLevelBuffer;

RWStructuredBuffer<float4> OutputBuffer;

Texture2D inputTexture;

cbuffer cbData : register(b0)
{
    uint TotalCount = 1;
	uint UvCount;
	uint LevelCount;
};


cbuffer cbSamplerData : register(b1)
{
    int MipLevel = 0;
};


[numthreads(64, 1, 1)]
void CS_ConstantLevel(uint3 DTid : SV_DispatchThreadID) 
{
    if (DTid.x >= TotalCount)
        return;

    int2 uv = uvBuffer[DTid.x];
    OutputBuffer[DTid.x] = inputTexture.Load(int3(uv, MipLevel));
}

[numthreads(64, 1, 1)]
void CS_DynamicLevel(uint3 DTid : SV_DispatchThreadID)
{
    if (DTid.x >= TotalCount)
        return;

    int2 uv = uvBuffer[DTid.x % UvCount];
	int level = uvLevelBuffer[DTid.x % LevelCount];
    OutputBuffer[DTid.x] = inputTexture.Load(int3(uv, level));
}



technique11 ConstantLevel 
{ 
    pass P0 
    { 
        SetComputeShader(CompileShader(cs_5_0, CS_ConstantLevel())); 
    } 
}

technique11 DynamicLevel 
{ 
    pass P0 
    { 
        SetComputeShader(CompileShader(cs_5_0, CS_DynamicLevel())); 
    } 
}



