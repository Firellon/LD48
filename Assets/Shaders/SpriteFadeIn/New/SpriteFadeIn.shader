// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SpriteFadeIn"
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
		_BorderMask("BorderMask", 2D) = "white" {}
		_FadeValue("FadeValue", Range( 0 , 1)) = 0
		_SketchMask("SketchMask", 2D) = "white" {}
		_Fade("Fade", Range( 0 , 1)) = 0
		_Float0("Float 0", Float) = 0
		_Float1("Float 1", Float) = 0
		_AddSketchValue("AddSketchValue", Range( -1 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}

	SubShader
	{
		LOD 0

		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
		
		Stencil
		{
			Ref [_Stencil]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
			CompFront [_StencilComp]
			PassFront [_StencilOp]
			FailFront Keep
			ZFailFront Keep
			CompBack Always
			PassBack Keep
			FailBack Keep
			ZFailBack Keep
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
			
			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			#define ASE_NEEDS_FRAG_COLOR

			
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
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				
			};
			
			uniform fixed4 _Color;
			uniform fixed4 _TextureSampleAdd;
			uniform float4 _ClipRect;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform sampler2D _SketchMask;
			uniform float4 _SketchMask_ST;
			uniform float _FadeValue;
			uniform float _Float0;
			uniform float _Float1;
			uniform float _AddSketchValue;
			uniform sampler2D _BorderMask;
			uniform float4 _BorderMask_ST;
			uniform float _Fade;

			
			v2f vert( appdata_t IN  )
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID( IN );
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				OUT.worldPosition = IN.vertex;
				
				
				OUT.worldPosition.xyz +=  float3( 0, 0, 0 ) ;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(v2f IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float2 uv_MainTex = IN.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode20 = tex2D( _MainTex, uv_MainTex );
				float2 uv_SketchMask = IN.texcoord.xy * _SketchMask_ST.xy + _SketchMask_ST.zw;
				float lerpResult21 = lerp( ( 1.0 - tex2D( _SketchMask, uv_SketchMask ).r ) , 1.0 , _FadeValue);
				float clampResult35 = clamp( lerpResult21 , _Float0 , _Float1 );
				float clampResult40 = clamp( ( clampResult35 + _AddSketchValue ) , 0.0 , 1.0 );
				float2 uv_BorderMask = IN.texcoord.xy * _BorderMask_ST.xy + _BorderMask_ST.zw;
				float4 appendResult9 = (float4(tex2DNode20.r , tex2DNode20.g , tex2DNode20.b , ( tex2DNode20.a * clampResult40 * IN.color.a * tex2D( _BorderMask, uv_BorderMask ).r * _Fade )));
				
				half4 color = appendResult9;
				
				#ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback "_StencilWriteMask"
}
/*ASEBEGIN
Version=18900
0;67;1512;838;1380.086;511.961;2.031078;True;True
Node;AmplifyShaderEditor.TexturePropertyNode;23;-978.5599,817.0369;Inherit;True;Property;_SketchMask;SketchMask;4;0;Create;True;0;0;0;False;0;False;None;5f9ad9f42050f44aa84b2853f5bafaa8;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;24;-630.8583,695.4371;Inherit;True;Property;_TextureSample3;Texture Sample 3;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;28;-298.8785,712.2521;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-472.8045,1036.265;Inherit;False;Property;_FadeValue;FadeValue;3;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;21;-16.67777,498.2008;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-277.2103,77.05163;Inherit;False;Property;_Float0;Float 0;7;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-254.8684,188.7609;Inherit;False;Property;_Float1;Float 1;8;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;70.10368,117.6731;Inherit;False;Property;_AddSketchValue;AddSketchValue;9;0;Create;True;0;0;0;False;0;False;0;-1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;35;-74.10233,38.46114;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;1;-922.2286,-433.8586;Inherit;False;0;0;_MainTex;Shader;False;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;38;96.50773,-63.09268;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;19;-1010.041,499.5299;Inherit;True;Property;_BorderMask;BorderMask;2;0;Create;True;0;0;0;False;0;False;None;5705dda99c4a74c43a85c7568a787ef9;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.VertexColorNode;17;-510.0466,242.7144;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;30;183.909,216.7658;Inherit;False;Property;_Fade;Fade;5;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;20;-682.1594,-497.7713;Inherit;True;Property;_TextureSample1;Texture Sample 1;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;18;-629.4456,459.5586;Inherit;True;Property;_TextureSample2;Texture Sample 2;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;40;553.5002,257.8176;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;484.0245,-100.168;Inherit;True;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;12;-996.1809,29.06846;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;32;111.2686,920.1588;Inherit;True;Property;_TextureSample4;Texture Sample 4;7;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;5;-328.041,-175.4019;Inherit;True;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.TexturePropertyNode;31;-91.17163,873.8687;Inherit;True;Property;_DiagonalMask;DiagonalMask;6;0;Create;True;0;0;0;False;0;False;None;55b6460a240e249829a9d91814c649e8;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.OneMinusNode;34;336.175,741.2147;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;3;-1004.968,-286.9027;Inherit;True;Property;_MaskTex;MaskTex;0;0;Create;True;0;0;0;False;0;False;None;eeb1b24255644bc4aaa483a1269cfa53;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;2;-659.6648,-185.8534;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;29;-1430.866,243.2159;Inherit;False;3;0;FLOAT;-1;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;33;622.5564,487.3289;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1456.361,-361.8163;Inherit;False;Property;_MaskOffset;MaskOffset;1;0;Create;True;0;0;0;False;0;False;0;-1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;9;777.3456,-208.2134;Inherit;False;COLOR;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-39.33838,-109.7167;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;10;-1423.142,26.64303;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;27;-1113.46,-74.74126;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;16;1030.736,-206.8288;Float;False;True;-1;2;ASEMaterialInspector;0;6;SpriteFadeIn;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;False;True;3;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;True;True;True;True;True;0;True;-9;False;False;False;False;False;False;False;True;True;0;True;-5;255;True;-8;255;True;-7;0;True;-4;0;True;-6;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;0;True;-11;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;0;False;False;False;False;False;False;False;False;False;True;False;False;False;True;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;True;2;0;_StencilWriteMask;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;24;0;23;0
WireConnection;28;0;24;1
WireConnection;21;0;28;0
WireConnection;21;2;22;0
WireConnection;35;0;21;0
WireConnection;35;1;36;0
WireConnection;35;2;37;0
WireConnection;38;0;35;0
WireConnection;38;1;39;0
WireConnection;20;0;1;0
WireConnection;18;0;19;0
WireConnection;40;0;38;0
WireConnection;8;0;20;4
WireConnection;8;1;40;0
WireConnection;8;2;17;4
WireConnection;8;3;18;1
WireConnection;8;4;30;0
WireConnection;12;0;27;0
WireConnection;12;1;10;0
WireConnection;32;0;31;0
WireConnection;5;0;2;0
WireConnection;34;0;32;1
WireConnection;2;0;3;0
WireConnection;2;1;12;0
WireConnection;29;2;22;0
WireConnection;33;0;21;0
WireConnection;33;1;34;0
WireConnection;9;0;20;1
WireConnection;9;1;20;2
WireConnection;9;2;20;3
WireConnection;9;3;8;0
WireConnection;27;1;29;0
WireConnection;16;0;9;0
ASEEND*/
//CHKSM=7DE8F2D01887BD32840D2E718A05C13C43FD3703