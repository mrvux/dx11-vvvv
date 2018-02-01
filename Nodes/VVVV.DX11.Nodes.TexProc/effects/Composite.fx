//@author: 
//@help: 
//@tags: mixer
//@credits:

Texture2D tex0 : INPUTTEXTURE;
Texture2D tex1 : SECONDTEXTURE;
SamplerState s0 <bool visible=false;string uiname="Sampler";>
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = WRAP;
    AddressV = WRAP;
};
float2 R : TARGETSIZE;

cbuffer controls : register(b0)
{
    float Opacity <float uimin=0.0;float uimax=1.0;> = 0.5;
};

#define bld(op,c0,c1) float4(lerp((c0*c0.a+c1*c1.a*(1-c0.a))/saturate(c0.a+c1.a*(1-c0.a)),(op),c0.a*c1.a).rgb,saturate(c0.a+c1.a*(1-c0.a)))

float4 pNORMAL(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld(lerp(c0,c1,Opacity), c1, c0);
    return c;
}
float4 pADD(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld(c0+c1, c0, c1);
    return c;
}
float4 pSUBTRACT(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld(c0-c1, c0, c1);
    return c;
}
float4 pSCREEN(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld(c0+c1*saturate(1-c0), c0, c1);
    return c;
}
float4 pMUL(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld(c0*c1, c0, c1);
    return c;
}
float4 pDARKEN(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld(min(c0,c1), c0, c1);
    return c;
}
float4 pLIGHTEN(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld(max(c0,c1), c0, c1);
    return c;
}
float4 pDIFFERENCE(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld(abs(c0-c1), c0, c1);
    return c;
}
float4 pEXCLUSION(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld(c0+c1-2*c0*c1, c0, c1);
    return c;
}
float4 pOVERLAY(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld((c0<.5)?(2*c0*c1):1-2*(1-c0)*(1-c1), c0, c1);
    return c;
}
float4 pHARDLIGHT(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld((c1<.5)?(2*c0*c1):1-2*(1-c0)*(1-c1), c0, c1);
    return c;
}
float4 pSOFTLIGHT(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld(2*c0*c1+c0*c0-2*c0*c0*c1, c0, c1);
    return c;
}
float4 pDODGE(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld((c1==1)?1:c0/(1-c1), c0, c1);
    return c;
}
float4 pBURN(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld((c1==0)?0:1-(1-c0)/c1, c0, c1);
    return c;
}
float4 pREFLECT(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld((c1==1)?1:c0*c0/(1-c1), c0, c1);
    return c;
}
float4 pGLOW(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld((c0==1)?1:c1*c1/(1-c0), c0, c1);
    return c;
}
float4 pFREEZE(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld((c1==0)?0:1-pow(1-c0,2)/c1, c0, c1);
    return c;
}
float4 pHEAT(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld((c0==0)?0:1-pow(1-c1,2)/c0, c0, c1);
    return c;
}
float4 pDIVIDE(float4 PosWVP : SV_POSITION, float2 x1 : TEXCOORD0, float2 x2 : TEXCOORD1) : SV_TARGET
{
    float4 c0 = tex0.Sample(s0, x1);
    float4 c1 = tex1.Sample(s0, x2) * float4(1, 1, 1, Opacity);
    float4 c = bld(c0/c1, c0, c1);
    return c;
}

technique10 Normal
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pNORMAL()));
    }
}
technique10 Screen
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pSCREEN()));
    }
}
technique10 Multiply
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pMUL()));
    }
}
technique10 Add
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pADD()));
    }
}
technique10 Subtract
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pSUBTRACT()));
    }
}
technique10 Darken
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pDARKEN()));
    }
}
technique10 Lighten
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pLIGHTEN()));
    }
}
technique10 Difference
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pDIFFERENCE()));
    }
}
technique10 Exclusion
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pEXCLUSION()));
    }
}
technique10 Overlay
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pOVERLAY()));
    }
}
technique10 Hardlight
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pHARDLIGHT()));
    }
}
technique10 Softlight
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pSOFTLIGHT()));
    }
}
technique10 Dodge
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pDODGE()));
    }
}
technique10 Burn
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pBURN()));
    }
}
technique10 Reflect
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pREFLECT()));
    }
}
technique10 Glow
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pGLOW()));
    }
}
technique10 Freeze
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pFREEZE()));
    }
}
technique10 Heat
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pHEAT()));
    }
}
technique10 Divide
{
    pass P1
    {
        SetPixelShader(CompileShader(ps_4_0, pDIVIDE()));
    }
}
