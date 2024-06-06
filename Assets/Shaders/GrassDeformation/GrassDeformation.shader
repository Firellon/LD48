Shader "Nebulate.me/GrassDeformation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _WindSpeed("_WindSpeed", Float) = 1.0
        _WindDirection("_WindDirection", Vector) = (1,0,0,0)
        _WindScale("_WindScale", Range(0, 10)) = 1.0
        _DistortionPower("_DistortionPower", Range(0, 1)) = 1.0

        _Noise("Noise", 2D) = "black" {}

        _Subtract("_Subtract", Float) = -0.5
        _Multiply("_Multiply", Float) = 2

        _GradScale("_GradScale", Range(0, 1)) = 1
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _Noise;

            float4 _MainTex_ST;

            half _WindSpeed;
            fixed2 _WindDirection;
            half _WindScale;
            half _DistortionPower;

            half _Subtract;
            half _Multiply;

            float2 _WorldPos;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float timeSin = sin(_Time.y + _WorldPos.x + _WorldPos.y);

                fixed2 noiseOffset = _WindDirection * _WindSpeed * timeSin;
                fixed2 noiseValue = ((tex2D(_Noise, i.worldPos * _WindScale + noiseOffset) - _Subtract) * _Multiply).xy * fixed2(timeSin, 0);

                fixed gradUV = i.uv.y;
                fixed2 grad = clamp(0, 1, pow(abs(gradUV), 1));

                // return pow(abs(gradUV), 1);

                fixed2 uv = i.uv;
                fixed2 distortionUV = noiseValue.xy * grad * _DistortionPower;

                fixed4 col = tex2D(_MainTex, uv + distortionUV);
                col.rgb *= col.a;

                return col;
            }
            ENDCG
        }
    }
}
