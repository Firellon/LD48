// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CloudsEffect"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin]_CloudsTexture("CloudsTexture", 2D) = "white" {}
		_MovementDirection("MovementDirection", Vector) = (1,0,0,0)
		_CloudsScale("CloudsScale", Float) = 0
		_CloudsPower("CloudsPower", Range( 0 , 1)) = 0
		_StepA("StepA", Range( 0 , 1)) = 0
		_StepB("StepB", Range( 0 , 1)) = 0
		_CloudsColor("CloudsColor", Color) = (0,0,0,0)
		_PixelizationData("PixelizationData", Vector) = (0,0,0,0)
		[ASEEnd]_Transparency("Transparency", Range( 0 , 1)) = 0

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

			

			sampler2D _CloudsTexture;
			CBUFFER_START( UnityPerMaterial )
			float4 _CloudsColor;
			float2 _MovementDirection;
			float2 _PixelizationData;
			float _StepA;
			float _StepB;
			float _CloudsPower;
			float _CloudsScale;
			float _Transparency;
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
				float3 break68 = ( ( ase_worldPos * ( 1.0 / _CloudsScale ) ) + float3( ( _TimeParameters.x * _MovementDirection ) ,  0.0 ) );
				float2 appendResult67 = (float2(( floor( ( break68.x * _PixelizationData.x ) ) / _PixelizationData.x ) , ( floor( ( break68.y * _PixelizationData.y ) ) / _PixelizationData.y )));
				float2 Input_UV145_g2 = appendResult67;
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
				float4 Output_2D293_g2 = ( ( tex2D( _CloudsTexture, UV12_g2, temp_output_10_0_g2, temp_output_12_0_g2 ) * W12_g2 ) + ( tex2D( _CloudsTexture, UV22_g2, temp_output_10_0_g2, temp_output_12_0_g2 ) * W22_g2 ) + ( tex2D( _CloudsTexture, UV32_g2, temp_output_10_0_g2, temp_output_12_0_g2 ) * W32_g2 ) );
				float smoothstepResult76 = smoothstep( _StepA , _StepB , ( _CloudsPower * Output_2D293_g2.r ));
				float4 appendResult41 = (float4(smoothstepResult76 , smoothstepResult76 , smoothstepResult76 , smoothstepResult76));
				float4 break79 = ( appendResult41 * _CloudsColor );
				float4 appendResult80 = (float4(break79.r , break79.g , break79.b , ( break79.a * _Transparency )));
				
				float4 Color = appendResult80;

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
137.1429;110.2857;1971.429;950.4286;1534.249;1347.301;1.3;True;False
Node;AmplifyShaderEditor.RangedFloatNode;31;-1917.231,-776.8658;Inherit;False;Property;_CloudsScale;CloudsScale;2;0;Create;True;0;0;0;False;0;False;0;30;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;34;-1943.039,-654.6968;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;3;-1941.458,-946.5853;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;32;-1669.515,-821.7255;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;35;-1944.238,-573.9067;Inherit;False;Property;_MovementDirection;MovementDirection;1;0;Create;True;0;0;0;False;0;False;1,0;0.01,0.001;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-1493.862,-902.7208;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-1663.156,-598.3121;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;37;-1268.706,-934.1439;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;68;-1383.449,-659.9725;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.Vector2Node;66;-1470.549,-435.0724;Inherit;False;Property;_PixelizationData;PixelizationData;7;0;Create;True;0;0;0;False;0;False;0,0;1200,1200;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-1175.449,-645.6726;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;-1135.582,-487.5059;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;70;-1042.849,-646.9727;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;73;-999.082,-481.006;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;74;-824.8824,-419.9059;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;71;-924.5491,-650.8726;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;1;-1420.652,-1187.894;Inherit;True;Property;_CloudsTexture;CloudsTexture;0;0;Create;True;0;0;0;False;0;False;55f6f84eb260e6d41b0ef06d442b049f;edcf24f9695428842b57e7f93236981c;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.DynamicAppendNode;67;-790.6489,-656.0726;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;62;-901.49,-1033.402;Inherit;False;Procedural Sample;-1;;2;f5379ff72769e2b4495e5ce2f004d8d4;2,157,0,315,0;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,0;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.BreakToComponentsNode;40;-687.4033,-1067.255;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;47;-872.7902,-1238.314;Inherit;False;Property;_CloudsPower;CloudsPower;3;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;-531.3511,-1085.452;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-585.249,-700.8293;Inherit;False;Property;_StepB;StepB;5;0;Create;True;0;0;0;False;0;False;0;0.6;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-694.2022,-858.5724;Inherit;False;Property;_StepA;StepA;4;0;Create;True;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;76;-365.5489,-1070.029;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;41;-47.69811,-1085.758;Inherit;False;COLOR;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;64;-204.349,-927.5863;Inherit;False;Property;_CloudsColor;CloudsColor;6;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.09433933,0.09433933,0.09433933,0.4;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;128.451,-1057.586;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;79;290.9509,-1058.887;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;77;99.85097,-826.1862;Inherit;False;Property;_Transparency;Transparency;8;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;415.751,-866.4863;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;80;523.6509,-1079.687;Inherit;False;COLOR;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;748.9925,-1094.945;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;12;CloudsEffect;cf964e524c8e69742b1d21fbe2ebcc4a;True;Unlit;0;0;Unlit;3;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;False;True;2;5;False;-1;10;False;-1;3;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;1;Vertex Position;1;0;1;True;False;;False;0
WireConnection;32;1;31;0
WireConnection;33;0;3;0
WireConnection;33;1;32;0
WireConnection;36;0;34;0
WireConnection;36;1;35;0
WireConnection;37;0;33;0
WireConnection;37;1;36;0
WireConnection;68;0;37;0
WireConnection;69;0;68;0
WireConnection;69;1;66;1
WireConnection;72;0;68;1
WireConnection;72;1;66;2
WireConnection;70;0;69;0
WireConnection;73;0;72;0
WireConnection;74;0;73;0
WireConnection;74;1;66;2
WireConnection;71;0;70;0
WireConnection;71;1;66;1
WireConnection;67;0;71;0
WireConnection;67;1;74;0
WireConnection;62;82;1;0
WireConnection;62;5;67;0
WireConnection;40;0;62;0
WireConnection;48;0;47;0
WireConnection;48;1;40;0
WireConnection;76;0;48;0
WireConnection;76;1;51;0
WireConnection;76;2;75;0
WireConnection;41;0;76;0
WireConnection;41;1;76;0
WireConnection;41;2;76;0
WireConnection;41;3;76;0
WireConnection;65;0;41;0
WireConnection;65;1;64;0
WireConnection;79;0;65;0
WireConnection;78;0;79;3
WireConnection;78;1;77;0
WireConnection;80;0;79;0
WireConnection;80;1;79;1
WireConnection;80;2;79;2
WireConnection;80;3;78;0
WireConnection;0;1;80;0
ASEEND*/
//CHKSM=6A2D4558D65B9AC7FD928CDD85A536E1504EDE8C