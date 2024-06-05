// #include "UnityShaderVariables.cginc"
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
// #pragma exclude_renderers gles

inline float rand_stochastic(float2 s) {
    return frac(sin(mul(float2x2( 127.1, 311.7, 269.5, 183.3), s)) * 43758.5453);
}

float4 tex2d_stochastic_float(sampler2D tex, float2 UV)
{
    float2 UV1; 
    float2 UV2;
    float2 UV3;
    float W1;
    float W2;
    float W3;

    float2 vertex1, vertex2, vertex3;
    // Scaling of the input
    float2 uv = UV * 3.464; // 2 * sqrt (3)
    // Skew input space into simplex triangle grid
    const float2x2 gridToSkewedGrid = float2x2( 1.0, 0.0, -0.57735027, 1.15470054 );
    float2 skewedCoord = mul( gridToSkewedGrid, uv );
    // Compute local triangle vertex IDs and local barycentric coordinates
    int2 baseId = int2( floor( skewedCoord ) );
    float3 temp = float3( frac( skewedCoord ), 0 );
    temp.z = 1.0 - temp.x - temp.y;

    if ( temp.z > 0.0 )
    {
        W1 = temp.z;
        W2 = temp.y;
        W3 = temp.x;
        vertex1 = baseId;
        vertex2 = baseId + int2( 0, 1 );
        vertex3 = baseId + int2( 1, 0 );
    }
    else
    {
        W1 = -temp.z;
        W2 = 1.0 - temp.y;
        W3 = 1.0 - temp.x;
        vertex1 = baseId + int2( 1, 1 );
        vertex2 = baseId + int2( 1, 0 );
        vertex3 = baseId + int2( 0, 1 );
    }
    UV1 = UV + rand_stochastic(vertex1);
    UV2 = UV + rand_stochastic(vertex2);
    UV3 = UV + rand_stochastic(vertex3);

    float2 uv_ddx = ddx(UV);
    float2 uv_ddy = ddy(UV);

    return (tex2D( tex, UV1, uv_ddx, uv_ddy ) * W1) +
        (tex2D(tex, UV2, uv_ddx, uv_ddy ) * W2) +
        (tex2D(tex, UV3, uv_ddx, uv_ddy) * W3);
}

inline float hash2D2D(float2 s)
{
    return frac(
        sin(fmod(float2(dot(s, float2(127.1, 311.7)), dot(s, float2(269.5, 183.3))), 3.14159265359)) * 43758.5453);
}

float random (float2 uv)
{
    return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
}

inline float3x3 uv_triplanar(float3 pos,float3 normal,float3 scale)
{
    float3 bf = normalize(abs(normal));
    bf /= dot(bf, (float3)1);

    float2 tx = pos.yz * scale;
    float2 ty = pos.zx * scale;
    float2 tz = pos.xy * scale;

    return float3x3(float3(tx, bf.x), float3(ty, bf.y), float3(tz, bf.z));
}

inline float3 uv_triplanar_xz(float3 pos,float3 normal,float3 scale)
{
    float3 bf = normalize(abs(normal));
    bf /= dot(bf, (float3)1);

    float2 ty = pos.xz * scale;

    return float3(ty, bf.y);
}


inline float4 tex2D_triplanar(sampler2D tex,float3 pos,float3 normal,float3 scale)
{
    float3x3 uv = uv_triplanar(pos, normal, scale);

    float4 cx;
    float4 cy;
    float4 cz;

    cx = tex2D(tex, uv[0].xy) * uv[0].z;
    cy = tex2D(tex, uv[1].xy) * uv[1].z;
    cz = tex2D(tex, uv[2].xy) * uv[2].z;

    return cx + cy + cz;
}

inline float4 tex2D_triplanar_xz(sampler2D tex,float3 pos,float3 normal,float3 scale)
{
    float3 uv = uv_triplanar_xz(pos, normal, scale);
    float4 c = tex2D(tex, uv.xy) * uv.z;
    return c;
}

inline float4 tex2D_triplanar_xy_yz(sampler2D tex,float3 pos,float3 normal,float3 scale)
{
    float3 bf = normalize(abs(normal));
    bf /= dot(bf, (float3)1);

    float2 tx = pos.yz * scale;
    float2 tz = pos.xy * scale;

    float4 cx = tex2D(tex, tx) * bf.x;
    float4 cz = tex2D(tex, tz) * bf.z;

    return cx + cz;
}

inline float4 tex2D_stochastic(sampler2D tex, float2 uv)
{
    float2 skewUV = mul(float2x2(1.0, 0.0, -0.57735027, 1.15470054), uv * 3.464);

    float2 vxID = float2(floor(skewUV));
    float3 barry = float3(frac(skewUV), 0);
    barry.z = 1.0 - barry.x - barry.y;

    float barryStep01 = step(0, barry.z);
    float barryStep11 = barryStep01 * 2 - 1;

    float3 x = float3(vxID + float2(1, 1) * (1 - barryStep01), 0);
    float3 y = float3(vxID + float2(1 - barryStep01, barryStep01), 0);
    float3 z = float3(vxID + float2(barryStep01, 1 - barryStep01), 0);
    float3 w = float3(barry.z * barryStep11,
                      (barry.y * barryStep11) + 1 - barryStep01,
                      (barry.x * barryStep11) + 1 - barryStep01);

    float4x3 BW_vx = float4x3(x, y, z, w);

    float2 dx = ddx(uv);
    float2 dy = ddy(uv);

    return mul(tex2D(tex, uv + hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) +
        mul(tex2D(tex, uv + hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) +
        mul(tex2D(tex, uv + hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z);
}

inline float4 tex2D_triplanar_stochastic(sampler2D tex, float3 pos, float3 normal,float scale)
{
    float3x3 uv = uv_triplanar(pos, normal, scale);

    float4 cx;
    float4 cy;
    float4 cz;

    cx = tex2D_stochastic(tex, uv[0].xy) * uv[0].z;
    cy = tex2D_stochastic(tex, uv[1].xy) * uv[1].z;
    cz = tex2D_stochastic(tex, uv[2].xy) * uv[2].z;

    return cx + cy + cz;
}

inline float4 tex2D_triplanar_stochastic_xz(sampler2D tex, float3 pos,float3 normal,float scale)
{
    float3 uv = uv_triplanar_xz(pos, normal, scale);
    float4 c = tex2D_stochastic(tex, uv.xy) * uv.z;
    return c;
}

inline float4 tex2D_triplanar_stochastic_xz_triplanar_xy_yz(sampler2D tex, float3 pos,float3 normal,float scale)
{
    float3x3 uv = uv_triplanar(pos, normal, scale);

    float4 cx = tex2D(tex, uv[0].xy) * uv[0].z;
    float4 cz = tex2D(tex, uv[2].xy) * uv[2].z;

    float4 c = tex2D_stochastic(tex, uv[1].xy) * uv[1].z;
    return c + cx + cz;
}

inline float aaStep(float compValue, float gradient){
    float floatChange = fwidth(gradient) / 2;
    //base the range of the inverse lerp on the change over one pixel
    float lowerEdge = compValue - floatChange;
    float upperEdge = compValue + floatChange;
    //do the inverse interpolation
    float stepped = (gradient - lowerEdge) / (upperEdge - lowerEdge);
    stepped = saturate(stepped);
    return stepped;
}