Shader "UI/HandDrawnEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        _Progress ("Progress", Range(0,1)) = 0.0
        _MaskTex ("Mask Texture", 2D) = "white" {}

        _DrawEffectStroke ("DrawEffectStroke", Range(0, 1)) = 0.1

        _Threshold1 ("Threshold1", Range(0.0, 0.3)) = 0.1
        _Threshold2 ("Threshold2", Range(0.0, 0.3)) = 0.1

        _RectMask ("RectMask", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _Progress;

            float _Threshold;

            sampler2D _MaskTex;
            float4 _MaskTex_ST;

            sampler2D _RectMask;

            half _DrawEffectStroke;

            half _Threshold1;
            half _Threshold2;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float strokeEffect(float2 uv, float progress, float threshold)
            {
                float diagonalPos = (1.0 - uv.x + uv.y) * 0.5;

                float maskValue = 1.0 - tex2D(_MaskTex, uv).r;

                return smoothstep(progress - threshold, progress + _Threshold1, diagonalPos) * maskValue;
            }

            float strokeEffectNeg(float2 uv, float progress, float threshold)
            {
                float diagonalPos = (1.0 - uv.x + uv.y) * 0.5;

                float maskValue = tex2D(_MaskTex, uv).r;

                return smoothstep(progress - threshold, progress + _Threshold2, diagonalPos) * maskValue;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                float drawEffectAlpha = strokeEffect(IN.texcoord, 1.0 - _Progress, _Threshold);
                float drawEffectNegativeAlpha = strokeEffectNeg(IN.texcoord, min(1.0, 1.0 - _Progress + _DrawEffectStroke), _Threshold);

                color.a *= (drawEffectAlpha + drawEffectNegativeAlpha);

                half4 rectMask = tex2D(_RectMask, IN.texcoord);

                color.a *= rectMask.r;

                return color;
            }
        ENDCG
        }
    }
}