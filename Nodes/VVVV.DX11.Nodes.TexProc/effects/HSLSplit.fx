Texture2D InputTexture : TEXTURE;

float4 rgb2hsl(float4 c)
{
	float red = c.r;
	float green = c.g;
	float blue = c.b;

    float v; float m; float vm;
    float r2, g2, b2;

    float hue = 0; // default to black
    float saturation = 0;
    float luminance = 0;

    v = max(red, green);
    v = max(v, blue);
    m = min(red, green);
    m = min(m, blue);
	
	luminance = (m + v) * 0.5f;
	
	if (luminance > 0.0f)
	{
		vm = v - m;
        saturation = vm;
		
		if (saturation > 0.0f)
        {
        	saturation /= (luminance <= 0.5f) ? (v + m) : (2.0f - v - m);
        	
        	r2 = (v - red) / vm;
            g2 = (v - green) / vm;
            b2 = (v - blue) / vm;
        	
        	if (red == v)
            {
                hue = (green == m ? 5.0f + b2 : 1.0f - g2);
            }
            else if (green == v)
            {
                hue = (blue == m ? 1.0f + r2 : 3.0f - b2);
            }
            else
            {
                hue = (red == m ? 3.0f + g2 : 5.0f - r2);
            }
            hue /= 6.0f;
            hue = hue >= 1.0f ? hue - 1.0f : hue;
        }
	}
	
	c.r = hue;
	c.g = saturation;
	c.b = luminance;
	return c;
	
}

float4 rgb2hsv(float4 c)
{
	float hue = 0;
    float saturation = 0;
    float value = 0;
	
	float red = c.r;
	float green = c.g;
	float blue = c.b;
	
	float mi = min(red, min(green, blue));
    value = max(red, max(green, blue));
    float delta = value - mi;
	
	if (delta == 0.0f || value == 0.0f)
    {
    	saturation = 0.0f;
        hue = 0.0f;    
    }
	else
	{
		saturation = delta / value;
		if (red == value) 	
        	hue = (green - blue) / (6.0f * delta);
        else if (green == value)					
            hue = (2.0f + (blue - red) / delta) / 6.0f;
        else			                    	
            hue = (4.0f + (red - green) / delta) / 6.0f;

        if (hue < 0.0f)
        {
        	hue = hue + 1.0f;
        }
	}
	
	c.r = hue;
	c.g = saturation;
	c.b = value;

	return c;
	
}

void PS_HSL_Single(float4 pixelPos : SV_POSITION, 
out float red : SV_Target0,
out float green : SV_Target1, 
out float blue : SV_Target2,
out float alpha : SV_Target3)
{
	float4 c = rgb2hsl(InputTexture.Load(int3(pixelPos.xy, 0)));
	red = c.r;
	green = c.g;
	blue = c.b;
	alpha = c.a;
}

void PS_HSL_Expand(float4 pixelPos : SV_POSITION,
    out float4 red : SV_Target0,
    out float4 green : SV_Target1,
    out float4 blue : SV_Target2,
    out float4 alpha : SV_Target3)
{
	float4 c = rgb2hsl(InputTexture.Load(int3(pixelPos.xy, 0)));
    red = c.rrrr;
    green = c.gggg;
    blue = c.bbbb;
    alpha = c.aaaa;
}

void PS_HSV_Single(float4 pixelPos : SV_POSITION,
    out float red : SV_Target0,
    out float green : SV_Target1,
    out float blue : SV_Target2,
    out float alpha : SV_Target3)
{
	float4 c = rgb2hsv(InputTexture.Load(int3(pixelPos.xy, 0)));
    red = c.r;
    green = c.g;
    blue = c.b;
    alpha = c.a;
}

void PS_HSV_Expand(float4 pixelPos : SV_POSITION,
    out float4 red : SV_Target0,
    out float4 green : SV_Target1,
    out float4 blue : SV_Target2,
    out float4 alpha : SV_Target3)
{
	float4 c = rgb2hsv(InputTexture.Load(int3(pixelPos.xy, 0)));
    red = c.rrrr;
    green = c.gggg;
    blue = c.bbbb;
    alpha = c.aaaa;
}

technique11 HSLSingle
{
	pass P0
	{
		SetPixelShader (CompileShader(ps_4_0, PS_HSL_Single()));
	}
}

technique11 HSLExpand
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_HSL_Expand()));
    }
}

technique11 HSVSingle
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_HSV_Single()));
    }
}

technique11 HSVExpand
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_4_0, PS_HSV_Expand()));
    }
}
