#include "UnityShaderVariables.cginc"
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles

inline fixed hash2D2D(float2 s)
{
    return frac(
        sin(fmod(fixed2(dot(s, fixed2(127.1, 311.7)), dot(s, fixed2(269.5, 183.3))), 3.14159265359)) * 43758.5453);
}

inline float3x3 uv_triplanar(fixed3 pos,fixed3 normal,fixed3 scale)
{
    fixed3 bf = normalize(abs(normal));
    bf /= dot(bf, (fixed3)1);

    fixed2 tx = pos.yz * scale;
    fixed2 ty = pos.zx * scale;
    fixed2 tz = pos.xy * scale;

    return float3x3(float3(tx, bf.x), float3(ty, bf.y), float3(tz, bf.z));
}

inline float3 uv_triplanar_xz(fixed3 pos,fixed3 normal,fixed3 scale)
{
    fixed3 bf = normalize(abs(normal));
    bf /= dot(bf, (fixed3)1);

    fixed2 ty = pos.xz * scale;

    return float3(ty, bf.y);
}


inline fixed4 tex2D_triplanar(sampler2D tex,fixed3 pos,fixed3 normal,fixed3 scale)
{
    float3x3 uv = uv_triplanar(pos, normal, scale);

    fixed4 cx;
    fixed4 cy;
    fixed4 cz;

    cx = tex2D(tex, uv[0].xy) * uv[0].z;
    cy = tex2D(tex, uv[1].xy) * uv[1].z;
    cz = tex2D(tex, uv[2].xy) * uv[2].z;

    return cx + cy + cz;
}

inline fixed4 tex2D_triplanar_xz(sampler2D tex,fixed3 pos,fixed3 normal,fixed3 scale)
{
    float3 uv = uv_triplanar_xz(pos, normal, scale);
    fixed4 c = tex2D(tex, uv.xy) * uv.z;
    return c;
}

inline fixed4 tex2D_triplanar_xy_yz(sampler2D tex,fixed3 pos,fixed3 normal,fixed3 scale)
{
    fixed3 bf = normalize(abs(normal));
    bf /= dot(bf, (fixed3)1);

    fixed2 tx = pos.yz * scale;
    fixed2 tz = pos.xy * scale;

    fixed4 cx = tex2D(tex, tx) * bf.x;
    fixed4 cz = tex2D(tex, tz) * bf.z;

    return cx + cz;
}

inline fixed4 tex2D_stochastic(sampler2D tex, fixed2 uv)
{
    fixed2 skewUV = mul(fixed2x2(1.0, 0.0, -0.57735027, 1.15470054), uv * 3.464);

    fixed2 vxID = fixed2(floor(skewUV));
    fixed3 barry = fixed3(frac(skewUV), 0);
    barry.z = 1.0 - barry.x - barry.y;

    fixed barryStep01 = step(0, barry.z);
    fixed barryStep11 = barryStep01 * 2 - 1;

    fixed3 x = fixed3(vxID + fixed2(1, 1) * (1 - barryStep01), 0);
    fixed3 y = fixed3(vxID + fixed2(1 - barryStep01, barryStep01), 0);
    fixed3 z = fixed3(vxID + fixed2(barryStep01, 1 - barryStep01), 0);
    fixed3 w = fixed3(barry.z * barryStep11,
                      (barry.y * barryStep11) + 1 - barryStep01,
                      (barry.x * barryStep11) + 1 - barryStep01);

    float4x3 BW_vx = float4x3(x, y, z, w);

    fixed2 dx = ddx(uv);
    fixed2 dy = ddy(uv);

    return mul(tex2D(tex, uv + hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) +
        mul(tex2D(tex, uv + hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) +
        mul(tex2D(tex, uv + hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z);
}

inline fixed4 tex2D_triplanar_stochastic(sampler2D tex, fixed3 pos, fixed3 normal,fixed scale)
{
    float3x3 uv = uv_triplanar(pos, normal, scale);

    fixed4 cx;
    fixed4 cy;
    fixed4 cz;

    cx = tex2D_stochastic(tex, uv[0].xy) * uv[0].z;
    cy = tex2D_stochastic(tex, uv[1].xy) * uv[1].z;
    cz = tex2D_stochastic(tex, uv[2].xy) * uv[2].z;

    return cx + cy + cz;
}

inline fixed4 tex2D_triplanar_stochastic_xz(sampler2D tex, fixed3 pos,fixed3 normal,fixed scale)
{
    float3 uv = uv_triplanar_xz(pos, normal, scale);
    fixed4 c = tex2D_stochastic(tex, uv.xy) * uv.z;
    return c;
}

inline fixed4 tex2D_triplanar_stochastic_xz_triplanar_xy_yz(sampler2D tex, fixed3 pos,fixed3 normal,fixed scale)
{
    float3x3 uv = uv_triplanar(pos, normal, scale);

    fixed4 cx = tex2D(tex, uv[0].xy) * uv[0].z;
    fixed4 cz = tex2D(tex, uv[2].xy) * uv[2].z;

    fixed4 c = tex2D_stochastic(tex, uv[1].xy) * uv[1].z;
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