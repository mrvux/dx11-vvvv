
cbuffer cbParams
{
	int WarpSize : WARPSIZE;
};

ByteAddressBuffer CounterBuffer : COUNTERBUFFER;

RWStructuredBuffer<uint> RWDispatchBuffer :  RWDISPATCHBUFFER;

[numthreads(1, 1, 1)]
void CS_GenerateDispatch(uint3 DTid : SV_DispatchThreadID)
{
	uint count = CounterBuffer.Load(0);

	RWDispatchBuffer[0] = (count + WarpSize - 1) / WarpSize;
	RWDispatchBuffer[1] = 1;
	RWDispatchBuffer[2] = 1;
}

technique11 GenerateDispatchBuffer
{
	pass P0
	{
		SetComputeShader(CompileShader(cs_5_0, CS_GenerateDispatch()));
	}
}