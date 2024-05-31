Shader "Nebulate.me/GrassDeformation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Value("Value", Range(-0.2, 0.2)) = 0
        _Angle("_Angle", Range(-90, 90)) = 0

        _WindSpeed("_WindSpeed", Float) = 1.0
        _WindDirection("_WindDirection", Vector) = (1,0,0,0)
        _WindScale("_WindScale", Range(0, 1)) = 1.0
        _DistortionPower("_DistortionPower", Range(0, 1)) = 1.0

        _Noise("Noise", 2D) = "black" {}
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
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _Noise;

            float4 _MainTex_ST;

            half _Value;
            half _Angle;

            half _WindSpeed;
            fixed2 _WindDirection;
            half _WindScale;
            half _DistortionPower;

            inline float gradient(float x)
			{
				//(-(2x-1)^2+1)
				// return -pow(2 * x - 1, 2) + 1;
                float sqr = x * x;
                return sqr / (2.0 * (sqr - x) + 1.0);

                return x;
			}

            v2f vert (appdata v)
            {
                v2f o;

                // if (v.vertex.y > 0.1)
                // {
                //     v.vertex.x += _Value; // * _SinTime.w
                // }

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed2 noiseOffset = _WindDirection * _Time.y * _WindSpeed;

                fixed2 noiseValue = (tex2D(_Noise, i.worldPos * _WindScale + noiseOffset) - 0.5) * 2 * fixed2(sin(_Time.w + i.worldPos.x), 0);//_WindDirection;

                fixed2 grad = clamp(0, 1, pow(abs(i.uv.y), 2));

                fixed4 col = tex2D(_MainTex, i.uv + noiseValue.xy * grad * _DistortionPower);
                col.rgb *= col.a;

                return col;

                // fixed2 pix = fixed2(600, 600);
                // fixed2 uv = fixed2(floor((i.uv.x) * pix.x) / pix.x, floor((i.uv.y) * pix.y) / pix.y);
                //
                // fixed4 col = tex2D(_MainTex, i.uv);
                // col.rgb *= col.a;
                //
                // return col;
            }
            ENDCG
        }
    }
}
