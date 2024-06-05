// #include "UnityShaderVariables.cginc"
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
// #pragma exclude_renderers gles

inline float hash2D2D(float2 s)
{
    return frac(
        sin(fmod(half2(dot(s, half2(127.1, 311.7)), dot(s, half2(269.5, 183.3))), 3.14159265359)) * 43758.5453);
}

float random (float2 uv)
{
    return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
}

inline float3x3 uv_triplanar(half3 pos,half3 normal,half3 scale)
{
    half3 bf = normalize(abs(normal));
    bf /= dot(bf, (half3)1);

    half2 tx = pos.yz * scale;
    half2 ty = pos.zx * scale;
    half2 tz = pos.xy * scale;

    return float3x3(float3(tx, bf.x), float3(ty, bf.y), float3(tz, bf.z));
}

inline float3 uv_triplanar_xz(half3 pos,half3 normal,half3 scale)
{
    half3 bf = normalize(abs(normal));
    bf /= dot(bf, (half3)1);

    half2 ty = pos.xz * scale;

    return float3(ty, bf.y);
}


inline half4 tex2D_triplanar(sampler2D tex,half3 pos,half3 normal,half3 scale)
{
    float3x3 uv = uv_triplanar(pos, normal, scale);

    half4 cx;
    half4 cy;
    half4 cz;

    cx = tex2D(tex, uv[0].xy) * uv[0].z;
    cy = tex2D(tex, uv[1].xy) * uv[1].z;
    cz = tex2D(tex, uv[2].xy) * uv[2].z;

    return cx + cy + cz;
}

inline half4 tex2D_triplanar_xz(sampler2D tex,half3 pos,half3 normal,half3 scale)
{
    float3 uv = uv_triplanar_xz(pos, normal, scale);
    half4 c = tex2D(tex, uv.xy) * uv.z;
    return c;
}

inline half4 tex2D_triplanar_xy_yz(sampler2D tex,half3 pos,half3 normal,half3 scale)
{
    half3 bf = normalize(abs(normal));
    bf /= dot(bf, (half3)1);

    half2 tx = pos.yz * scale;
    half2 tz = pos.xy * scale;

    half4 cx = tex2D(tex, tx) * bf.x;
    half4 cz = tex2D(tex, tz) * bf.z;

    return cx + cz;
}

inline half4 tex2D_stochastic(sampler2D tex, half2 uv)
{
    half2 skewUV = mul(half2x2(1.0, 0.0, -0.57735027, 1.15470054), uv * 3.464);

    half2 vxID = half2(floor(skewUV));
    half3 barry = half3(frac(skewUV), 0);
    barry.z = 1.0 - barry.x - barry.y;

    half barryStep01 = step(0, barry.z);
    half barryStep11 = barryStep01 * 2 - 1;

    half3 x = half3(vxID + half2(1, 1) * (1 - barryStep01), 0);
    half3 y = half3(vxID + half2(1 - barryStep01, barryStep01), 0);
    half3 z = half3(vxID + half2(barryStep01, 1 - barryStep01), 0);
    half3 w = half3(barry.z * barryStep11,
                      (barry.y * barryStep11) + 1 - barryStep01,
                      (barry.x * barryStep11) + 1 - barryStep01);

    float4x3 BW_vx = float4x3(x, y, z, w);

    half2 dx = ddx(uv);
    half2 dy = ddy(uv);

    return mul(tex2D(tex, uv + hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) +
        mul(tex2D(tex, uv + hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) +
        mul(tex2D(tex, uv + hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z);
}

inline half4 tex2D_triplanar_stochastic(sampler2D tex, half3 pos, half3 normal,half scale)
{
    float3x3 uv = uv_triplanar(pos, normal, scale);

    half4 cx;
    half4 cy;
    half4 cz;

    cx = tex2D_stochastic(tex, uv[0].xy) * uv[0].z;
    cy = tex2D_stochastic(tex, uv[1].xy) * uv[1].z;
    cz = tex2D_stochastic(tex, uv[2].xy) * uv[2].z;

    return cx + cy + cz;
}

inline half4 tex2D_triplanar_stochastic_xz(sampler2D tex, half3 pos,half3 normal,half scale)
{
    float3 uv = uv_triplanar_xz(pos, normal, scale);
    half4 c = tex2D_stochastic(tex, uv.xy) * uv.z;
    return c;
}

inline half4 tex2D_triplanar_stochastic_xz_triplanar_xy_yz(sampler2D tex, half3 pos,half3 normal,half scale)
{
    float3x3 uv = uv_triplanar(pos, normal, scale);

    half4 cx = tex2D(tex, uv[0].xy) * uv[0].z;
    half4 cz = tex2D(tex, uv[2].xy) * uv[2].z;

    half4 c = tex2D_stochastic(tex, uv[1].xy) * uv[1].z;
    return c + cx + cz;
}

inline float aaStep(float compValue, float gradient){
    float halfChange = fwidth(gradient) / 2;
    //base the range of the inverse lerp on the change over one pixel
    float lowerEdge = compValue - halfChange;
    float upperEdge = compValue + halfChange;
    //do the inverse interpolation
    float stepped = (gradient - lowerEdge) / (upperEdge - lowerEdge);
    stepped = saturate(stepped);
    return stepped;
}