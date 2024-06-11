// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "WindEffect"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin]_NoiseTexture("NoiseTexture", 2D) = "white" {}
		_DistortionDirection("DistortionDirection", Vector) = (1,0,0,0)
		_NoiseDirection("NoiseDirection", Vector) = (1,0,0,0)
		_DistortionTexture("DistortionTexture", 2D) = "white" {}
		_DistortionScale("DistortionScale", Float) = 0
		_NoiseScale("NoiseScale", Float) = 0
		_DistortionPower("DistortionPower", Range( 0 , 1)) = 0
		_WindPower("WindPower", Range( 0 , 1)) = 0
		_StepA("StepA", Range( 0 , 1)) = 0
		_StepB("StepB", Range( 0 , 1)) = 0
		_YSinMultiplier("YSinMultiplier", Range( 0 , 1)) = 0
		[ASEEnd]_YSinAmplitude("YSinAmplitude", Float) = 1

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

			

			sampler2D _DistortionTexture;
			sampler2D _NoiseTexture;
			CBUFFER_START( UnityPerMaterial )
			float2 _DistortionDirection;
			float2 _NoiseDirection;
			float _StepA;
			float _StepB;
			float _WindPower;
			float _DistortionScale;
			float _DistortionPower;
			float _NoiseScale;
			float _YSinAmplitude;
			float _YSinMultiplier;
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
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#if ETC1_EXTERNAL_ALPHA
				TEXTURE2D( _AlphaTex ); SAMPLER( sampler_AlphaTex );
				float _EnableAlphaTexture;
			#endif

			float4 _RendererColor;

			void StochasticTiling( float2 UV, out float2 UV1, out float2 UV2, out float2 UV3, out float W1, out float W2, out float W3 )
			{
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
				UV1 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex1 ) ) * 43758.5453 );
				UV2 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex2 ) ) * 43758.5453 );
				UV3 = UV + frac( sin( mul( float2x2( 127.1, 311.7, 269.5, 183.3 ), vertex3 ) ) * 43758.5453 );
				return;
			}
			

			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				o.ase_texcoord2.xyz = ase_worldPos;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.w = 0;
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

				float localStochasticTiling2_g2 = ( 0.0 );
				float3 ase_worldPos = IN.ase_texcoord2.xyz;
				float localStochasticTiling2_g1 = ( 0.0 );
				float2 Input_UV145_g1 = ( ( ase_worldPos * ( 1.0 / _NoiseScale ) ) + float3( ( _TimeParameters.x * _NoiseDirection ) ,  0.0 ) ).xy;
				float2 UV2_g1 = Input_UV145_g1;
				float2 UV12_g1 = float2( 0,0 );
				float2 UV22_g1 = float2( 0,0 );
				float2 UV32_g1 = float2( 0,0 );
				float W12_g1 = 0.0;
				float W22_g1 = 0.0;
				float W32_g1 = 0.0;
				StochasticTiling( UV2_g1 , UV12_g1 , UV22_g1 , UV32_g1 , W12_g1 , W22_g1 , W32_g1 );
				float2 temp_output_10_0_g1 = ddx( Input_UV145_g1 );
				float2 temp_output_12_0_g1 = ddy( Input_UV145_g1 );
				float4 Output_2D293_g1 = ( ( tex2D( _NoiseTexture, UV12_g1, temp_output_10_0_g1, temp_output_12_0_g1 ) * W12_g1 ) + ( tex2D( _NoiseTexture, UV22_g1, temp_output_10_0_g1, temp_output_12_0_g1 ) * W22_g1 ) + ( tex2D( _NoiseTexture, UV32_g1, temp_output_10_0_g1, temp_output_12_0_g1 ) * W32_g1 ) );
				float4 break31_g1 = Output_2D293_g1;
				float2 appendResult69 = (float2(0.0 , ( sin( ( _TimeParameters.x * _YSinAmplitude ) ) * _YSinMultiplier )));
				float2 Input_UV145_g2 = ( ( ( ( ase_worldPos * ( 1.0 / _DistortionScale ) ) + float3( ( _TimeParameters.x * _DistortionDirection ) ,  0.0 ) ) + ( _DistortionPower * break31_g1.r ) ) + float3( appendResult69 ,  0.0 ) ).xy;
				float2 UV2_g2 = Input_UV145_g2;
				float2 UV12_g2 = float2( 0,0 );
				float2 UV22_g2 = float2( 0,0 );
				float2 UV32_g2 = float2( 0,0 );
				float W12_g2 = 0.0;
				float W22_g2 = 0.0;
				float W32_g2 = 0.0;
				StochasticTiling( UV2_g2 , UV12_g2 , UV22_g2 , UV32_g2 , W12_g2 , W22_g2 , W32_g2 );
				float2 temp_output_10_0_g2 = ddx( Input_UV145_g2 );
				float2 temp_output_12_0_g2 = ddy( Input_UV145_g2 );
				float4 Output_2D293_g2 = ( ( tex2D( _DistortionTexture, UV12_g2, temp_output_10_0_g2, temp_output_12_0_g2 ) * W12_g2 ) + ( tex2D( _DistortionTexture, UV22_g2, temp_output_10_0_g2, temp_output_12_0_g2 ) * W22_g2 ) + ( tex2D( _DistortionTexture, UV32_g2, temp_output_10_0_g2, temp_output_12_0_g2 ) * W32_g2 ) );
				float smoothstepResult50 = smoothstep( _StepA , _StepB , ( _WindPower * Output_2D293_g2.r ));
				float4 appendResult41 = (float4(smoothstepResult50 , smoothstepResult50 , smoothstepResult50 , smoothstepResult50));
				
				float4 Color = appendResult41;

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
0;85.14286;2173.714;1076.714;2948.032;869.5262;1.428613;True;False
Node;AmplifyShaderEditor.RangedFloatNode;59;-2586.81,-771.601;Inherit;False;Property;_NoiseScale;NoiseScale;5;0;Create;True;0;0;0;False;0;False;0;15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;58;-2647.411,-967.0815;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector2Node;54;-2622.929,-522.0042;Inherit;False;Property;_NoiseDirection;NoiseDirection;2;0;Create;True;0;0;0;False;0;False;1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;56;-2618.872,-637.0811;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;60;-2354.873,-790.8673;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-2297.56,-650.6984;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;73;-1822.285,84.78748;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-1836.572,173.3614;Inherit;False;Property;_YSinAmplitude;YSinAmplitude;11;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-2220.304,-849.4379;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-2132.845,-319.764;Inherit;False;Property;_DistortionScale;DistortionScale;4;0;Create;True;0;0;0;False;0;False;0;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;34;-2158.653,-197.5951;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;3;-2157.072,-489.4836;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;-1606.565,100.5022;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;35;-2159.852,-116.805;Inherit;False;Property;_DistortionDirection;DistortionDirection;1;0;Create;True;0;0;0;False;0;False;1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TexturePropertyNode;1;-2197.881,-1093.233;Inherit;True;Property;_NoiseTexture;NoiseTexture;0;0;Create;True;0;0;0;False;0;False;55f6f84eb260e6d41b0ef06d442b049f;edcf24f9695428842b57e7f93236981c;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;57;-2020.249,-838.5062;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;32;-1885.129,-364.6238;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;72;-1457.989,89.07336;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;70;-1815.142,292.1399;Inherit;False;Property;_YSinMultiplier;YSinMultiplier;10;0;Create;True;0;0;0;False;0;False;0;0.01;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;53;-1778.812,-912.5894;Inherit;False;Procedural Sample;-1;;1;f5379ff72769e2b4495e5ce2f004d8d4;2,157,0,315,0;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,0;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.RangedFloatNode;45;-1879.578,-1058.41;Inherit;False;Property;_DistortionPower;DistortionPower;6;0;Create;True;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-1878.77,-141.2104;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-1709.476,-445.619;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-1235.125,99.27716;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;37;-1450.12,-378.2422;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-1476.663,-888.1633;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;69;-1073.692,-140.7298;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;44;-1300.92,-545.9371;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;19;-1850.821,-1518.371;Inherit;True;Property;_DistortionTexture;DistortionTexture;3;0;Create;True;0;0;0;False;0;False;None;22d85a8aed710b041ab7905d0b87b584;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;67;-1000.833,-482.1683;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;62;-1023.69,-963.2026;Inherit;False;Procedural Sample;-1;;2;f5379ff72769e2b4495e5ce2f004d8d4;2,157,0,315,0;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,0;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.RangedFloatNode;47;-872.7902,-1238.314;Inherit;False;Property;_WindPower;WindPower;7;0;Create;True;0;0;0;False;0;False;0;0.75;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;40;-762.8033,-1060.755;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;52;-913.0733,-647.6838;Inherit;False;Property;_StepB;StepB;9;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-916.5022,-751.9724;Inherit;False;Property;_StepA;StepA;8;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;-531.3511,-1085.452;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;50;-503.6331,-857.6898;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;41;-277.7981,-1067.558;Inherit;False;COLOR;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;39;-1451.838,-1173.459;Inherit;True;Property;_TextureSample26;Texture Sample 26;11;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;-46.60744,-905.1448;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;12;WindEffect;cf964e524c8e69742b1d21fbe2ebcc4a;True;Unlit;0;0;Unlit;3;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;False;True;2;5;False;-1;10;False;-1;3;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;1;Vertex Position;1;0;1;True;False;;False;0
WireConnection;60;1;59;0
WireConnection;55;0;56;0
WireConnection;55;1;54;0
WireConnection;61;0;58;0
WireConnection;61;1;60;0
WireConnection;75;0;73;0
WireConnection;75;1;74;0
WireConnection;57;0;61;0
WireConnection;57;1;55;0
WireConnection;32;1;31;0
WireConnection;72;0;75;0
WireConnection;53;82;1;0
WireConnection;53;5;57;0
WireConnection;36;0;34;0
WireConnection;36;1;35;0
WireConnection;33;0;3;0
WireConnection;33;1;32;0
WireConnection;71;0;72;0
WireConnection;71;1;70;0
WireConnection;37;0;33;0
WireConnection;37;1;36;0
WireConnection;46;0;45;0
WireConnection;46;1;53;32
WireConnection;69;1;71;0
WireConnection;44;0;37;0
WireConnection;44;1;46;0
WireConnection;67;0;44;0
WireConnection;67;1;69;0
WireConnection;62;82;19;0
WireConnection;62;5;67;0
WireConnection;40;0;62;0
WireConnection;48;0;47;0
WireConnection;48;1;40;0
WireConnection;50;0;48;0
WireConnection;50;1;51;0
WireConnection;50;2;52;0
WireConnection;41;0;50;0
WireConnection;41;1;50;0
WireConnection;41;2;50;0
WireConnection;41;3;50;0
WireConnection;39;0;19;0
WireConnection;0;1;41;0
ASEEND*/
//CHKSM=8670DD5804552AC0A31EF0D28C88CB08E36CB0C0