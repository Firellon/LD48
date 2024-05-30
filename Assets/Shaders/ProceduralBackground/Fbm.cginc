float random (float2 n, float seed)
{
    // return frac(sin(dot(n, float2(123.456789, 987.654321))) * 54321.9876 );
    return frac(sin(dot(n, float2(12.9898, 78.233))) * seed);
}

float noise(float2 p, float seed)
{
    float2 i = floor(p);
    float2 u = smoothstep(0.0, 1.0, frac(p));
    float a = random(i + float2(0,0), seed);
    float b = random(i + float2(1,0), seed);
    float c = random(i + float2(0,1), seed);
    float d = random(i + float2(1,1), seed);
    float r = lerp(lerp(a, b, u.x),lerp(c, d, u.x), u.y);
    return r * r;
}

float fbm(float2 uv, int octaves, float gain, float lacunarity, float seed)
{
    float value = 0.0;
    float amplitude = gain;

    for (int i = 0; i < octaves; ++ i)
    {
        value += amplitude * noise(uv, seed); 
        uv *= lacunarity; 
        amplitude *= gain; 
    }

    return value;
}

float maskSelector(float moduleA, float moduleB, float moduleC, float selectPoint, float falloff)
{
    float diff = selectPoint - falloff;

    if (moduleC >= selectPoint)
    {
        return moduleB;
    }
    else if (moduleC <= diff)
    {
        return moduleA;
    }
    else
    {
        return lerp(moduleA, moduleB, (1.0 / ((selectPoint - diff) / (selectPoint - moduleC))));
    }
}

float mixColors(float moduleA, float moduleB, float moduleC)
{
    return moduleA * (1.0 - moduleC) + moduleB * moduleC;
}

half4 findNearestColorInPalette(sampler2D paletteTex, int maxColors, half4 color)
{
    float colorOffset = 1.0 / maxColors;

    half4 resultColor = half4(0,0,0,1);
    half dist = 10000000.0;

    half2 uv = half2(0,0);

    for (int i = 0; i < maxColors; i++)
    {
        uv.x = i * colorOffset + 0.0001;
        half4 paletteColor = tex2D(paletteTex, uv);

        half distBetweenColors = distance(color, paletteColor);
        if (distBetweenColors < dist)
        {
            dist = distBetweenColors;
            resultColor = paletteColor;
        }
    }

    resultColor.a = color.a;

    return resultColor;
}

float4 dither(float4 color, float3 uv)
{
    float DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    return color - DITHER_THRESHOLDS[index];
}