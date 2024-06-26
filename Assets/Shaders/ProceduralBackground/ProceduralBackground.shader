Shader "Nebulate.me/ProceduralBackgroundWithDithering"
{
    Properties
	{
        _MainTex("MainTex", 2D) = "white" {}
        _DitherTex("_DitherTex", 2D) = "black" {}
        _PaletteTex("_PaletteTex", 2D) = "black" {}
        _QuantTex("_QuantTex", 2D) = "black" {}
        _MeasureTex("_MeasureTex", 2D) = "black" {}

	    _Dither("Power", Range(0, 1)) = 0
        _Weight("Impact", Range(0, 1)) = 0

	    _PatternData("_PatternData", Vector) = (0,0,0,0)
        _DitherMad("_DitherMad", Vector) = (0,0,0,0)

	    _NoiseTex("NoiseTex", 2D) = "black" {}

	    _LightColor("LightColor", Color) = (1,1,1,1)
	    _DarkColor("DarkColor", Color) = (1,1,1,1)

	    _NoiseScale("NoiseScale", Float) = 1.0

	    _StepA("StepA", Range(0, 1)) = 0.0
	    _StepB("StepB", Range(0, 1)) = 1.0

        // _PixelSize("PixelSize", Float) = 96.0

	    [Toggle] _Pixelate("Pixelate", Int) = 1
    }
    SubShader
    {
		LOD 0
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

		Cull Off
		HLSLINCLUDE
		#pragma target 2.0
		ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			// #include "Assets/Shaders/ProceduralBackground/stochastic.cginc"
			#include "Assets/Shaders/ProceduralBackground/stochastic.hlsl"

            #define LUT_SIZE 64.
            #define LUT_SIZE_MINUS (64. - 1.)

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

	        sampler2D _DitherTex;
            float4 _DitherTex_TexelSize;

	        sampler2D _PaletteTex;
	        sampler2D _QuantTex;
	        sampler2D _MeasureTex;

	        sampler2D _NoiseTex;

            float _NoiseScale;

            float4 _LightColor;
            float4 _DarkColor;

            float _StepA;
            float _StepB;

            uniform float _Dither;
            uniform float _Weight;

            uniform float4 _DitherMad;
            uniform float4 _PatternData;

            bool _Pixelate;

            float4 lut_sample(in float3 uvw, const sampler2D tex)
            {
                float2 uv;

                // get replacement color from the lut set
                uv.y = uvw.y * (LUT_SIZE_MINUS / LUT_SIZE) + .5 * (1. / LUT_SIZE);
                uv.x = uvw.x * (LUT_SIZE_MINUS / (LUT_SIZE * LUT_SIZE)) + .5 * (1. / (LUT_SIZE * LUT_SIZE)) + floor(uvw.z * LUT_SIZE) / LUT_SIZE;    

                float4 lutColor = tex2D(tex, uv);

#if !defined(UNITY_COLORSPACE_GAMMA)
                lutColor = float4(SRGBToLinear(lutColor.xyz), lutColor.w);
#endif

                return lutColor;
            }

            float4 grad_sample(in float2 uv, in float val, const sampler2D tex)
            {
                // xy - pix * aspect, z - sample scale, w - sample count
                uv.x *= _PatternData.z;
                uv.x += floor(val / _PatternData.z) * _PatternData.z;

                return tex2D(tex, uv);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = GetVertexPositionInputs(v.vertex.xyz).positionCS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // return tex2d_stochastic_float(_NoiseTex, i.worldPos.xy * (1.0 / _NoiseScale));

                float4 patternData =  _PatternData; //i.patternData;
                float4 ditherMad =  _DitherMad; //i.ditherMad;

                float2 pix = float2(patternData.x, patternData.y);
                float2 worldUV = i.worldPos.xy * (1.0 / _NoiseScale);

                float2 pixelatedWorldUV;

                if (_Pixelate)
                {
                    pixelatedWorldUV = float2(floor(worldUV.x * pix.x) / pix.x, floor(worldUV.y * pix.y) / pix.y);
                }
                else
                {
                    pixelatedWorldUV = worldUV;
                }

                float4 noiseBackground = tex2d_stochastic_float(_NoiseTex, pixelatedWorldUV);
                noiseBackground = lerp(_LightColor, _DarkColor, smoothstep(_StepA, _StepB, noiseBackground.r));

                float4 col = noiseBackground;

#if !defined(UNITY_COLORSPACE_GAMMA)
                float3 uvw = LinearToSRGB(noiseBackground).xyz;
#else
                float3 uvw = noiseBackground.xyz;
#endif

                float measure = lut_sample(uvw, _MeasureTex);
                float grade   = 1 - saturate(pow(1 - measure / _Dither, 4) + .001); // can be customized and remaped via curve (dither pattern sample)
                float noise   = grad_sample(frac(mad(worldUV, ditherMad.xy, ditherMad.zw)), grade, _DitherTex).r;

                float4 result = lerp(lut_sample(uvw, _PaletteTex), lut_sample(uvw, _QuantTex), step(measure, noise * _Dither));
                result.a *= col.a;

                return lerp(col, result, _Weight);
            }
            ENDHLSL
        }
    }
}