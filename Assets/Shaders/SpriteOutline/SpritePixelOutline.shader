// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Nebulate/SpritePixelOutline"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin]_MainTex("MainTex", 2D) = "white" {}
		_TexelsOffset("TexelsOffset", Int) = 0
		[ASEEnd]_BorderColor("BorderColor", Color) = (1,0,0,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

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
			Name "Unlit"
			

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#define ASE_SRP_VERSION 999999

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA

			#define _SURFACE_TYPE_TRANSPARENT 1
			#define SHADERPASS_SPRITEUNLIT

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			

			sampler2D _MainTex;
			CBUFFER_START( UnityPerMaterial )
			float4 _MainTex_ST;
			half4 _BorderColor;
			float4 _MainTex_TexelSize;
			int _TexelsOffset;
			CBUFFER_END


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 color : COLOR;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 texCoord0 : TEXCOORD0;
				float4 color : TEXCOORD1;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#if ETC1_EXTERNAL_ALPHA
				TEXTURE2D( _AlphaTex ); SAMPLER( sampler_AlphaTex );
				float _EnableAlphaTexture;
			#endif

			float4 _RendererColor;

			
			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3( 0, 0, 0 );
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.normal = v.normal;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.vertex.xyz );

				o.texCoord0 = v.uv0;
				o.color = v.color;
				o.clipPos = vertexInput.positionCS;

				return o;
			}

			half4 frag( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float2 uv_MainTex = IN.texCoord0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode12 = tex2D( _MainTex, uv_MainTex );
				float4 temp_cast_0 = (( 1.0 - tex2DNode12.a )).xxxx;
				float4 clampResult69 = clamp( ( tex2DNode12 - temp_cast_0 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float2 appendResult6 = (float2((float)_TexelsOffset , 0.0));
				float2 appendResult6_g93 = (float2(_MainTex_TexelSize.z , _MainTex_TexelSize.w));
				float2 appendResult8 = (float2((float)( _TexelsOffset * -1 ) , 0.0));
				float2 appendResult6_g95 = (float2(_MainTex_TexelSize.z , _MainTex_TexelSize.w));
				float2 appendResult7 = (float2(0.0 , (float)_TexelsOffset));
				float2 appendResult6_g92 = (float2(_MainTex_TexelSize.z , _MainTex_TexelSize.w));
				float2 appendResult5 = (float2(0.0 , (float)( _TexelsOffset * -1 )));
				float2 appendResult6_g94 = (float2(_MainTex_TexelSize.z , _MainTex_TexelSize.w));
				float clampResult15 = clamp( ( tex2DNode12.a + step( 0.01 , tex2D( _MainTex, ( uv_MainTex + ( appendResult6 / appendResult6_g93 ) ) ).a ) + step( 0.01 , tex2D( _MainTex, ( uv_MainTex + ( appendResult8 / appendResult6_g95 ) ) ).a ) + step( 0.01 , tex2D( _MainTex, ( uv_MainTex + ( appendResult7 / appendResult6_g92 ) ) ).a ) + step( 0.01 , tex2D( _MainTex, ( uv_MainTex + ( appendResult5 / appendResult6_g94 ) ) ).a ) ) , 0.0 , 1.0 );
				float4 appendResult65 = (float4(( clampResult69 + ( _BorderColor * ( clampResult15 - clampResult69.a ) ) )));
				
				float4 Color = appendResult65;

				#if ETC1_EXTERNAL_ALPHA
					float4 alpha = SAMPLE_TEXTURE2D( _AlphaTex, sampler_AlphaTex, IN.texCoord0.xy );
					Color.a = lerp( Color.a, alpha.r, _EnableAlphaTexture );
				#endif

				Color *= IN.color;

				return Color;
			}

			ENDHLSL
		}
	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18900
10.85714;94.85715;2031.429;1070.429;-331.7433;908.5037;1;True;False
Node;AmplifyShaderEditor.TexturePropertyNode;1;-1224.32,-664.7474;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;0;False;0;False;None;654346f8434b81349afc9a03e2e0e6f4;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.IntNode;2;-1129.818,-185.607;Inherit;False;Property;_TexelsOffset;TexelsOffset;1;0;Create;True;0;0;0;False;0;False;0;1;True;0;1;INT;0
Node;AmplifyShaderEditor.SamplerNode;12;-419.3567,-818.0447;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;43ad51e043491b94d8f6c62d55ac5ede;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-851.7297,-237.4553;Inherit;False;2;2;0;INT;0;False;1;INT;-1;False;1;INT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-854.4321,191.6259;Inherit;False;2;2;0;INT;0;False;1;INT;-1;False;1;INT;0
Node;AmplifyShaderEditor.DynamicAppendNode;6;-727.9885,-376.1141;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;5;-707.0731,191.6259;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;7;-752.5997,-24.71016;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;67;-105.5294,-758.1639;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;8;-698.9616,-221.0656;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;68;-21.4231,-1116.891;Inherit;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;61;-527.7515,-230.4891;Inherit;False;SpriteTexelOutlineBorder;-1;;95;674e7963dd58f954eb565ff316b82da9;0;2;11;FLOAT2;0,0;False;9;SAMPLER2D;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;63;-536.0812,181.167;Inherit;False;SpriteTexelOutlineBorder;-1;;94;674e7963dd58f954eb565ff316b82da9;0;2;11;FLOAT2;0,0;False;9;SAMPLER2D;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;62;-508.7034,-388.5393;Inherit;False;SpriteTexelOutlineBorder;-1;;93;674e7963dd58f954eb565ff316b82da9;0;2;11;FLOAT2;0,0;False;9;SAMPLER2D;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;64;-527.835,-13.97505;Inherit;False;SpriteTexelOutlineBorder;-1;;92;674e7963dd58f954eb565ff316b82da9;0;2;11;FLOAT2;0,0;False;9;SAMPLER2D;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;14;132.7124,-420.7509;Inherit;True;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;69;189.4999,-1122.01;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;15;379.4151,-410.0651;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;70;445.1699,-792.0601;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleSubtractOpNode;43;619.4323,-680.0566;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;56;635.7159,-328.6833;Half;False;Property;_BorderColor;BorderColor;2;0;Create;True;0;0;0;False;0;False;1,0,0,1;0,0.756525,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;858.7262,-628.3785;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;54;1094.239,-665.4301;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;65;1305.984,-555.2566;Inherit;True;COLOR;4;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;76;1642.071,-570.9003;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;13;Nebulate/SpritePixelOutline;cf964e524c8e69742b1d21fbe2ebcc4a;True;Unlit;0;0;Unlit;3;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;False;True;2;5;False;-1;10;False;-1;3;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;1;Vertex Position;1;0;1;True;False;;False;0
WireConnection;12;0;1;0
WireConnection;3;0;2;0
WireConnection;4;0;2;0
WireConnection;6;0;2;0
WireConnection;5;1;4;0
WireConnection;7;1;2;0
WireConnection;67;0;12;4
WireConnection;8;0;3;0
WireConnection;68;0;12;0
WireConnection;68;1;67;0
WireConnection;61;11;8;0
WireConnection;61;9;1;0
WireConnection;63;11;5;0
WireConnection;63;9;1;0
WireConnection;62;11;6;0
WireConnection;62;9;1;0
WireConnection;64;11;7;0
WireConnection;64;9;1;0
WireConnection;14;0;12;4
WireConnection;14;1;62;0
WireConnection;14;2;61;0
WireConnection;14;3;64;0
WireConnection;14;4;63;0
WireConnection;69;0;68;0
WireConnection;15;0;14;0
WireConnection;70;0;69;0
WireConnection;43;0;15;0
WireConnection;43;1;70;3
WireConnection;55;0;56;0
WireConnection;55;1;43;0
WireConnection;54;0;69;0
WireConnection;54;1;55;0
WireConnection;65;0;54;0
WireConnection;76;1;65;0
ASEEND*/
//CHKSM=43D6E02984956E73566E99684690DDF120703CAB