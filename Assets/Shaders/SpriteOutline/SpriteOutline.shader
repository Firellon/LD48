// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Nebulate/SpriteOutline"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin]_BorderColor("BorderColor", Color) = (1,0,0,1)
		_MainTex("MainTex", 2D) = "white" {}
		_BorderOffset("BorderOffset", Float) = 0
		_DiagonalsMultiplyer("DiagonalsMultiplyer", Float) = 0
		[ASEEnd]_SidesMultiplyer("SidesMultiplyer", Float) = 1.1
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
			float _BorderOffset;
			float _SidesMultiplyer;
			float _DiagonalsMultiplyer;
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
				float4 tex2DNode25 = tex2D( _MainTex, uv_MainTex );
				float4 temp_cast_0 = (( 1.0 - tex2DNode25.a )).xxxx;
				float4 clampResult15 = clamp( ( tex2DNode25 - temp_cast_0 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float2 appendResult11 = (float2(_BorderOffset , 0.0));
				float2 appendResult18 = (float2(( _BorderOffset * -1.0 ) , 0.0));
				float2 appendResult8 = (float2(0.0 , _BorderOffset));
				float2 appendResult12 = (float2(0.0 , ( _BorderOffset * -1.0 )));
				float2 appendResult47 = (float2(_BorderOffset , _BorderOffset));
				float temp_output_50_0 = ( _BorderOffset * -1.0 );
				float2 appendResult49 = (float2(temp_output_50_0 , temp_output_50_0));
				float temp_output_51_0 = ( _BorderOffset * -1.0 );
				float2 appendResult52 = (float2(_BorderOffset , temp_output_51_0));
				float2 appendResult53 = (float2(temp_output_51_0 , _BorderOffset));
				float clampResult10 = clamp( ( tex2DNode25.a + step( 0.01 , tex2D( _MainTex, ( uv_MainTex + ( appendResult11 * _SidesMultiplyer ) ) ).a ) + step( 0.01 , tex2D( _MainTex, ( uv_MainTex + ( appendResult18 * _SidesMultiplyer ) ) ).a ) + step( 0.01 , tex2D( _MainTex, ( uv_MainTex + ( appendResult8 * _SidesMultiplyer ) ) ).a ) + step( 0.01 , tex2D( _MainTex, ( uv_MainTex + ( appendResult12 * _SidesMultiplyer ) ) ).a ) + step( 0.01 , tex2D( _MainTex, ( uv_MainTex + ( appendResult47 * _DiagonalsMultiplyer ) ) ).a ) + step( 0.01 , tex2D( _MainTex, ( uv_MainTex + ( appendResult49 * _DiagonalsMultiplyer ) ) ).a ) + step( 0.01 , tex2D( _MainTex, ( uv_MainTex + ( appendResult52 * _DiagonalsMultiplyer ) ) ).a ) + step( 0.01 , tex2D( _MainTex, ( uv_MainTex + ( appendResult53 * _DiagonalsMultiplyer ) ) ).a ) ) , 0.0 , 1.0 );
				float4 appendResult7 = (float4(( clampResult15 + ( _BorderColor * ( clampResult10 - clampResult15.a ) ) )));
				
				float4 Color = appendResult7;

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
0;1233.714;480;269.8571;402.9825;800.787;1;False;False
Node;AmplifyShaderEditor.RangedFloatNode;38;-1374.472,251.8725;Inherit;False;Property;_BorderOffset;BorderOffset;2;0;Create;True;0;0;0;False;0;False;0;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-1113.361,203.7937;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-1453.184,1464.125;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-1128.784,440.8009;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-1328.354,1233.79;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;31;-1491.521,-704.461;Inherit;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;0;False;0;False;None;87b4bf819862aa646bfa577b4841375e;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;25;-1154.333,-707.3408;Inherit;True;Property;_Texture;Texture;0;0;Create;True;0;0;0;False;0;False;-1;None;ef9ee11f528ef2347bfe34225dd19acf;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;52;-1175.913,1470.883;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;11;-1121.096,-5.420403;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;8;-994.6777,337.2496;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;12;-945.0819,518.9678;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;47;-1130.496,1032.191;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-1300.383,1886.198;Inherit;False;Property;_DiagonalsMultiplyer;DiagonalsMultiplyer;3;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-1502.84,-185.5782;Inherit;False;Property;_SidesMultiplyer;SidesMultiplyer;4;0;Create;True;0;0;0;False;0;False;1.1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;53;-1111.587,1750.548;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;49;-1124.096,1275.384;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;18;-951.5679,159.5018;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-951.8443,1028.158;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;1.2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;64;-763.4835,102.5987;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-892.0278,1753.206;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;1.2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-944.0278,1534.206;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;1.2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;19;-150.3322,-418.414;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-776.4838,512.0986;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;-961.0278,1332.206;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;1.2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-858.3832,-54.70129;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-743.9839,378.1986;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;22;-291.9,-803.9873;Inherit;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;46;-748.6316,910.7167;Inherit;True;SpriteOutlineBorder;-1;;135;9ed76089818510e44a318efec1837132;0;2;11;FLOAT2;0,0;False;9;SAMPLER2D;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;55;-736.1227,1385.88;Inherit;True;SpriteOutlineBorder;-1;;133;9ed76089818510e44a318efec1837132;0;2;11;FLOAT2;0,0;False;9;SAMPLER2D;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;54;-729.7225,1629.074;Inherit;True;SpriteOutlineBorder;-1;;134;9ed76089818510e44a318efec1837132;0;2;11;FLOAT2;0,0;False;9;SAMPLER2D;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;48;-742.2313,1153.91;Inherit;True;SpriteOutlineBorder;-1;;136;9ed76089818510e44a318efec1837132;0;2;11;FLOAT2;0,0;False;9;SAMPLER2D;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;32;-554.7116,372.5188;Inherit;True;SpriteOutlineBorder;-1;;129;9ed76089818510e44a318efec1837132;0;2;11;FLOAT2;0,0;False;9;SAMPLER2D;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;34;-560.7119,117.319;Inherit;True;SpriteOutlineBorder;-1;;137;9ed76089818510e44a318efec1837132;0;2;11;FLOAT2;0,0;False;9;SAMPLER2D;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;35;-691.1312,-184.0951;Inherit;True;SpriteOutlineBorder;-1;;131;9ed76089818510e44a318efec1837132;0;2;11;FLOAT2;0,0;False;9;SAMPLER2D;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;33;-559.2117,614.7191;Inherit;True;SpriteOutlineBorder;-1;;130;9ed76089818510e44a318efec1837132;0;2;11;FLOAT2;0,0;False;9;SAMPLER2D;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-134.5146,-272.4881;Inherit;True;9;9;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;15;14.02299,-802.1063;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;10;102.188,-266.8022;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;28;356.0513,-438.9101;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ColorNode;5;525.839,-24.37963;Half;False;Property;_BorderColor;BorderColor;0;0;Create;True;0;0;0;False;0;False;1,0,0,1;0,0.03296852,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;9;584.9554,-334.1529;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;817.2493,-226.4748;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;13;1079.262,-251.9265;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;7;1303.507,-250.5529;Inherit;True;COLOR;4;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;39;1628.491,-250.0261;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;13;Nebulate/SpriteOutline;cf964e524c8e69742b1d21fbe2ebcc4a;True;Unlit;0;0;Unlit;3;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;False;True;2;5;False;-1;10;False;-1;3;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;1;Vertex Position;1;0;1;True;False;;False;0
WireConnection;29;0;38;0
WireConnection;51;0;38;0
WireConnection;14;0;38;0
WireConnection;50;0;38;0
WireConnection;25;0;31;0
WireConnection;52;0;38;0
WireConnection;52;1;51;0
WireConnection;11;0;38;0
WireConnection;8;1;38;0
WireConnection;12;1;14;0
WireConnection;47;0;38;0
WireConnection;47;1;38;0
WireConnection;53;0;51;0
WireConnection;53;1;38;0
WireConnection;49;0;50;0
WireConnection;49;1;50;0
WireConnection;18;0;29;0
WireConnection;59;0;47;0
WireConnection;59;1;61;0
WireConnection;64;0;18;0
WireConnection;64;1;62;0
WireConnection;58;0;53;0
WireConnection;58;1;61;0
WireConnection;57;0;52;0
WireConnection;57;1;61;0
WireConnection;19;0;25;4
WireConnection;66;0;12;0
WireConnection;66;1;62;0
WireConnection;56;0;49;0
WireConnection;56;1;61;0
WireConnection;63;0;11;0
WireConnection;63;1;62;0
WireConnection;65;0;8;0
WireConnection;65;1;62;0
WireConnection;22;0;25;0
WireConnection;22;1;19;0
WireConnection;46;11;59;0
WireConnection;46;9;31;0
WireConnection;55;11;57;0
WireConnection;55;9;31;0
WireConnection;54;11;58;0
WireConnection;54;9;31;0
WireConnection;48;11;56;0
WireConnection;48;9;31;0
WireConnection;32;11;65;0
WireConnection;32;9;31;0
WireConnection;34;11;64;0
WireConnection;34;9;31;0
WireConnection;35;11;63;0
WireConnection;35;9;31;0
WireConnection;33;11;66;0
WireConnection;33;9;31;0
WireConnection;16;0;25;4
WireConnection;16;1;35;0
WireConnection;16;2;34;0
WireConnection;16;3;32;0
WireConnection;16;4;33;0
WireConnection;16;5;46;0
WireConnection;16;6;48;0
WireConnection;16;7;55;0
WireConnection;16;8;54;0
WireConnection;15;0;22;0
WireConnection;10;0;16;0
WireConnection;28;0;15;0
WireConnection;9;0;10;0
WireConnection;9;1;28;3
WireConnection;23;0;5;0
WireConnection;23;1;9;0
WireConnection;13;0;15;0
WireConnection;13;1;23;0
WireConnection;7;0;13;0
WireConnection;39;1;7;0
ASEEND*/
//CHKSM=CC73B79510149A82A102DBED3287B992033C6CF2