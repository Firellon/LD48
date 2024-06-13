// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Nebulate.me/FogTest"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin]_NoiseTex("NoiseTex", 2D) = "white" {}
		_NoiseScale("NoiseScale", Float) = 1
		_MainColor("MainColor", Color) = (1,1,1,0.5019608)
		[ASEEnd]_NoiseMovement("NoiseMovement", Vector) = (0,0,0,0)

	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

		Cull Off
		HLSLINCLUDE
		#pragma target 3.0
		ENDHLSL

		
		Pass
		{
			Name "Unlit"
			

			Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
			ZTest Equal
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

			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"


			sampler2D _NoiseTex;
			CBUFFER_START( UnityPerMaterial )
			float4 _NoiseTex_ST;
			float4 _MainColor;
			float2 _NoiseMovement;
			float _NoiseScale;
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

			
			float4 SampleGradient( Gradient gradient, float time )
			{
				float3 color = gradient.colors[0].rgb;
				UNITY_UNROLL
				for (int c = 1; c < 8; c++)
				{
				float colorPos = saturate((time - gradient.colors[c-1].w) / ( 0.00001 + (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, gradient.colorsLength-1));
				color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
				}
				#ifndef UNITY_COLORSPACE_GAMMA
				color = SRGBToLinear(color);
				#endif
				float alpha = gradient.alphas[0].x;
				UNITY_UNROLL
				for (int a = 1; a < 8; a++)
				{
				float alphaPos = saturate((time - gradient.alphas[a-1].y) / ( 0.00001 + (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, gradient.alphasLength-1));
				alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
				}
				return float4(color, alpha);
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

				float2 temp_output_27_0 = ( _TimeParameters.x * _NoiseMovement );
				float3 ase_worldPos = IN.ase_texcoord2.xyz;
				float4 break36 = tex2D( _NoiseTex, ( float3( temp_output_27_0 ,  0.0 ) + ( ase_worldPos * ( 1.0 / _NoiseScale ) ) ).xy );
				Gradient gradient22 = NewGradient( 0, 2, 3, float4( 1, 1, 1, 0 ), float4( 1, 1, 1, 1 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 0.5000076 ), float2( 0, 1 ), 0, 0, 0, 0, 0 );
				float2 uv_NoiseTex = IN.texCoord0.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
				float4 appendResult37 = (float4(break36.r , break36.g , break36.b , ( break36.a * SampleGradient( gradient22, uv_NoiseTex.y ).a * _MainColor.a )));
				
				float4 Color = appendResult37;

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
110.8571;198.8571;1971.429;966.4286;716.9849;623.5978;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;8;-1458.857,197.3571;Inherit;False;Property;_NoiseScale;NoiseScale;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;6;-1429.457,-28.94287;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;26;-1430.857,354.4998;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;11;-1232.857,174.0712;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;29;-1406.857,483.4998;Inherit;False;Property;_NoiseMovement;NoiseMovement;3;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-1039.557,-6.242859;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-1201.857,343.4998;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;30;-811.5572,32.9998;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;4;-1120.657,-326.7428;Inherit;True;Property;_NoiseTex;NoiseTex;0;0;Create;True;0;0;0;False;0;False;55f6f84eb260e6d41b0ef06d442b049f;55f6f84eb260e6d41b0ef06d442b049f;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;5;-518.9572,-410.0429;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GradientNode;22;-270.8572,158.7855;Inherit;False;0;2;3;1,1,1,0;1,1,1,1;1,0;1,0.5000076;0,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;12;-867.8572,492.9283;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;9;-206.8572,-575.9428;Inherit;False;Property;_MainColor;MainColor;2;0;Create;True;0;0;0;False;0;False;1,1,1,0.5019608;1,1,1,0.5019608;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;36;115.1997,-361.9749;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GradientSampleNode;25;-49.85719,161.7855;Inherit;True;2;0;OBJECT;;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;314.4427,-129.8429;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;20;-274.0327,635.4521;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;16;-642.8572,379.9283;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;37;463.1997,-289.9749;Inherit;False;COLOR;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;82.8428,-148.8858;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;35;-384.7998,243.0253;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ClampOpNode;13;-181.7828,370.595;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;135.1428,525.7854;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-130.8572,-146.7858;Inherit;False;Property;_NoiseMultiplier;NoiseMultiplier;4;0;Create;True;0;0;0;False;0;False;0;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;15;-343.8944,356.595;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;31;-227.8572,-68.64301;Inherit;False;Procedural Sample;-1;;1;f5379ff72769e2b4495e5ce2f004d8d4;2,157,0,315,0;7;82;SAMPLER2D;0;False;158;SAMPLER2DARRAY;0;False;183;FLOAT;0;False;5;FLOAT2;0,0;False;80;FLOAT3;0,0,0;False;104;FLOAT2;1,0;False;74;SAMPLERSTATE;0;False;5;COLOR;0;FLOAT;32;FLOAT;33;FLOAT;34;FLOAT;35
Node;AmplifyShaderEditor.SimpleMaxOpNode;39;414.6144,119.7576;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;14;-490.8944,371.595;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;19;-619.1444,626.4521;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;18;-477.1443,633.4521;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;3;662.9,-283;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;12;Nebulate.me/FogTest;cf964e524c8e69742b1d21fbe2ebcc4a;True;Unlit;0;0;Unlit;3;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;0;True;True;2;5;False;-1;10;False;-1;2;5;False;8;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;True;True;False;0;False;-1;255;False;-1;255;False;-1;5;False;-1;4;False;-1;1;False;-1;1;False;-1;5;False;-1;4;False;-1;1;False;-1;1;False;-1;True;True;2;False;-1;True;5;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;1;Vertex Position;1;0;1;True;False;;False;0
WireConnection;11;1;8;0
WireConnection;7;0;6;0
WireConnection;7;1;11;0
WireConnection;27;0;26;0
WireConnection;27;1;29;0
WireConnection;30;0;27;0
WireConnection;30;1;7;0
WireConnection;5;0;4;0
WireConnection;5;1;30;0
WireConnection;12;2;4;0
WireConnection;36;0;5;0
WireConnection;25;0;22;0
WireConnection;25;1;12;2
WireConnection;10;0;36;3
WireConnection;10;1;25;4
WireConnection;10;2;9;4
WireConnection;20;0;18;0
WireConnection;16;0;12;2
WireConnection;37;0;36;0
WireConnection;37;1;36;1
WireConnection;37;2;36;2
WireConnection;37;3;10;0
WireConnection;33;0;32;0
WireConnection;33;1;31;32
WireConnection;35;0;27;0
WireConnection;13;0;15;0
WireConnection;21;0;13;0
WireConnection;21;1;20;0
WireConnection;15;0;14;0
WireConnection;31;82;4;0
WireConnection;14;0;16;0
WireConnection;19;0;12;2
WireConnection;18;0;19;0
WireConnection;3;1;37;0
ASEEND*/
//CHKSM=E659B09D5EA315E0D8AB4E10AB37BC0A496B1349