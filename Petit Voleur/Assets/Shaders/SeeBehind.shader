//Made by making shader graph, getting the generated code and then changing ZTest to always
//it don't work very well tho so
Shader "Custom/SeeBehind"
{
	Properties
	{
		[NoScaleOffset] Texture2D_118B5EB3("Base Map", 2D) = "white" {}
		Color_F65BA171("Base Color", Color) = (1, 1, 1, 1)
		[NoScaleOffset]Texture2D_A73B07CA("Normal Map", 2D) = "black" {}
		Vector1_83E209B8("Normal Multiplier", Range(0, 1)) = 1
		[NoScaleOffset]Texture2D_9971ADF4("Metallic Map", 2D) = "white" {}
		Vector1_8BFBC7FE("Metallic Multiplier", Range(0, 1)) = 0
		Vector1_B0F3CB11("Smoothness", Range(0, 1)) = 0.5
		[NoScaleOffset]Texture2D_67E365CE("Occlusion Map", 2D) = "white" {}
		Vector1_5A1B61E1("Occlusion Multiplier", Range(0, 1)) = 0
		Vector2_E513D0B4("Offset", Vector) = (0, 0, 0, 0)
		Vector2_1ACA2B2("Tiling", Vector) = (1, 1, 0, 0)
		Color_163D05C0("Behind Color", Color) = (1, 0, 0, 1)
	}
		SubShader
		{
			Tags
			{
				"RenderPipeline" = "UniversalPipeline"
				"RenderType" = "Transparent"
				"Queue" = "Transparent+0"
			}

			Pass
			{
				Name "Universal Forward"
				Tags
				{
					"LightMode" = "UniversalForward"
				}

			// Render State
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			Cull Back
			ZTest Always
			ZWrite Off
			// ColorMask: <None>


			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			// Debug
			// <None>

			// --------------------------------------------------
			// Pass

			// Pragmas
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0
			#pragma multi_compile_fog
			#pragma multi_compile_instancing

			// Keywords
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			// GraphKeywords: <None>

			// Defines
			#define _SURFACE_TYPE_TRANSPARENT 1
			#define _NORMAL_DROPOFF_TS 1
			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define ATTRIBUTES_NEED_TEXCOORD0
			#define ATTRIBUTES_NEED_TEXCOORD1
			#define VARYINGS_NEED_POSITION_WS 
			#define VARYINGS_NEED_NORMAL_WS
			#define VARYINGS_NEED_TANGENT_WS
			#define VARYINGS_NEED_TEXCOORD0
			#define VARYINGS_NEED_VIEWDIRECTION_WS
			#define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
			#pragma multi_compile_instancing
			#define SHADERPASS_FORWARD
			#define REQUIRE_DEPTH_TEXTURE


			// Includes
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

			// --------------------------------------------------
			// Graph

			// Graph Properties
			CBUFFER_START(UnityPerMaterial)
			float4 Color_F65BA171;
			float Vector1_83E209B8;
			float Vector1_8BFBC7FE;
			float Vector1_B0F3CB11;
			float Vector1_5A1B61E1;
			float2 Vector2_E513D0B4;
			float2 Vector2_1ACA2B2;
			float4 Color_163D05C0;
			CBUFFER_END
			TEXTURE2D(Texture2D_118B5EB3); SAMPLER(samplerTexture2D_118B5EB3); float4 Texture2D_118B5EB3_TexelSize;
			TEXTURE2D(Texture2D_A73B07CA); SAMPLER(samplerTexture2D_A73B07CA); float4 Texture2D_A73B07CA_TexelSize;
			TEXTURE2D(Texture2D_9971ADF4); SAMPLER(samplerTexture2D_9971ADF4); float4 Texture2D_9971ADF4_TexelSize;
			TEXTURE2D(Texture2D_67E365CE); SAMPLER(samplerTexture2D_67E365CE); float4 Texture2D_67E365CE_TexelSize;
			SAMPLER(_SampleTexture2D_5A082E38_Sampler_3_Linear_Repeat);
			SAMPLER(_SampleTexture2D_E017CDAF_Sampler_3_Linear_Repeat);
			SAMPLER(_SampleTexture2D_43D5697C_Sampler_3_Linear_Repeat);

			// Graph Functions

			void Unity_Negate_float(float In, out float Out)
			{
				Out = -1 * In;
			}

			void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
			{
				Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
			}

			void Unity_Comparison_LessOrEqual_float(float A, float B, out float Out)
			{
				Out = A <= B ? 1 : 0;
			}

			void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
			{
				Out = UV * Tiling + Offset;
			}

			void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
			{
				Out = A * B;
			}

			void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
			{
				Out = Predicate ? True : False;
			}

			// Graph Vertex
			// GraphVertex: <None>

			// Graph Pixel
			struct SurfaceDescriptionInputs
			{
				float3 TangentSpaceNormal;
				float3 ViewSpacePosition;
				float3 WorldSpacePosition;
				float4 ScreenPosition;
				float4 uv0;
			};

			struct SurfaceDescription
			{
				float3 Albedo;
				float3 Normal;
				float3 Emission;
				float Metallic;
				float Smoothness;
				float Occlusion;
				float Alpha;
				float AlphaClipThreshold;
			};

			SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
			{
				SurfaceDescription surface = (SurfaceDescription)0;
				float _Split_5525744F_R_1 = IN.ViewSpacePosition[0];
				float _Split_5525744F_G_2 = IN.ViewSpacePosition[1];
				float _Split_5525744F_B_3 = IN.ViewSpacePosition[2];
				float _Split_5525744F_A_4 = 0;
				float _Negate_280F0620_Out_1;
				Unity_Negate_float(_Split_5525744F_B_3, _Negate_280F0620_Out_1);
				float _SceneDepth_3EBB0BC0_Out_1;
				Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3EBB0BC0_Out_1);
				float _Comparison_8FF820DD_Out_2;
				Unity_Comparison_LessOrEqual_float(_Negate_280F0620_Out_1, _SceneDepth_3EBB0BC0_Out_1, _Comparison_8FF820DD_Out_2);
				float4 _Property_379B64D3_Out_0 = Color_F65BA171;
				float2 _Property_F46A7F06_Out_0 = Vector2_1ACA2B2;
				float2 _Property_693B57AB_Out_0 = Vector2_E513D0B4;
				float2 _TilingAndOffset_A5689075_Out_3;
				Unity_TilingAndOffset_float(IN.uv0.xy, _Property_F46A7F06_Out_0, _Property_693B57AB_Out_0, _TilingAndOffset_A5689075_Out_3);
				float4 _SampleTexture2D_5A082E38_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_118B5EB3, samplerTexture2D_118B5EB3, _TilingAndOffset_A5689075_Out_3);
				float _SampleTexture2D_5A082E38_R_4 = _SampleTexture2D_5A082E38_RGBA_0.r;
				float _SampleTexture2D_5A082E38_G_5 = _SampleTexture2D_5A082E38_RGBA_0.g;
				float _SampleTexture2D_5A082E38_B_6 = _SampleTexture2D_5A082E38_RGBA_0.b;
				float _SampleTexture2D_5A082E38_A_7 = _SampleTexture2D_5A082E38_RGBA_0.a;
				float4 _Multiply_FDC78828_Out_2;
				Unity_Multiply_float(_Property_379B64D3_Out_0, _SampleTexture2D_5A082E38_RGBA_0, _Multiply_FDC78828_Out_2);
				float4 _Property_6B9A620A_Out_0 = Color_163D05C0;
				float4 _Branch_7A8C5EB7_Out_3;
				Unity_Branch_float4(_Comparison_8FF820DD_Out_2, _Multiply_FDC78828_Out_2, _Property_6B9A620A_Out_0, _Branch_7A8C5EB7_Out_3);
				float _Property_C973DB48_Out_0 = Vector1_8BFBC7FE;
				float2 _Property_ABDEAFC7_Out_0 = Vector2_1ACA2B2;
				float2 _Property_C4FBDE05_Out_0 = Vector2_E513D0B4;
				float2 _TilingAndOffset_4D91BC85_Out_3;
				Unity_TilingAndOffset_float(IN.uv0.xy, _Property_ABDEAFC7_Out_0, _Property_C4FBDE05_Out_0, _TilingAndOffset_4D91BC85_Out_3);
				float4 _SampleTexture2D_E017CDAF_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_9971ADF4, samplerTexture2D_9971ADF4, _TilingAndOffset_4D91BC85_Out_3);
				float _SampleTexture2D_E017CDAF_R_4 = _SampleTexture2D_E017CDAF_RGBA_0.r;
				float _SampleTexture2D_E017CDAF_G_5 = _SampleTexture2D_E017CDAF_RGBA_0.g;
				float _SampleTexture2D_E017CDAF_B_6 = _SampleTexture2D_E017CDAF_RGBA_0.b;
				float _SampleTexture2D_E017CDAF_A_7 = _SampleTexture2D_E017CDAF_RGBA_0.a;
				float4 _Multiply_E83EB6C8_Out_2;
				Unity_Multiply_float((_Property_C973DB48_Out_0.xxxx), _SampleTexture2D_E017CDAF_RGBA_0, _Multiply_E83EB6C8_Out_2);
				float _Property_153B1DCB_Out_0 = Vector1_B0F3CB11;
				float _Property_A17CE3BA_Out_0 = Vector1_5A1B61E1;
				float2 _Property_2FF3513A_Out_0 = Vector2_1ACA2B2;
				float2 _Property_967B80F6_Out_0 = Vector2_E513D0B4;
				float2 _TilingAndOffset_242F4924_Out_3;
				Unity_TilingAndOffset_float(IN.uv0.xy, _Property_2FF3513A_Out_0, _Property_967B80F6_Out_0, _TilingAndOffset_242F4924_Out_3);
				float4 _SampleTexture2D_43D5697C_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_67E365CE, samplerTexture2D_67E365CE, _TilingAndOffset_242F4924_Out_3);
				float _SampleTexture2D_43D5697C_R_4 = _SampleTexture2D_43D5697C_RGBA_0.r;
				float _SampleTexture2D_43D5697C_G_5 = _SampleTexture2D_43D5697C_RGBA_0.g;
				float _SampleTexture2D_43D5697C_B_6 = _SampleTexture2D_43D5697C_RGBA_0.b;
				float _SampleTexture2D_43D5697C_A_7 = _SampleTexture2D_43D5697C_RGBA_0.a;
				float4 _Multiply_823673D5_Out_2;
				Unity_Multiply_float((_Property_A17CE3BA_Out_0.xxxx), _SampleTexture2D_43D5697C_RGBA_0, _Multiply_823673D5_Out_2);
				surface.Albedo = (_Branch_7A8C5EB7_Out_3.xyz);
				surface.Normal = IN.TangentSpaceNormal;
				surface.Emission = IsGammaSpace() ? float3(0, 0, 0) : SRGBToLinear(float3(0, 0, 0));
				surface.Metallic = (_Multiply_E83EB6C8_Out_2).x;
				surface.Smoothness = _Property_153B1DCB_Out_0;
				surface.Occlusion = (_Multiply_823673D5_Out_2).x;
				surface.Alpha = 1;
				surface.AlphaClipThreshold = 0;
				return surface;
			}

			// --------------------------------------------------
			// Structs and Packing

			// Generated Type: Attributes
			struct Attributes
			{
				float3 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float4 uv0 : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
				#if UNITY_ANY_INSTANCING_ENABLED
				uint instanceID : INSTANCEID_SEMANTIC;
				#endif
			};

			// Generated Type: Varyings
			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float3 positionWS;
				float3 normalWS;
				float4 tangentWS;
				float4 texCoord0;
				float3 viewDirectionWS;
				#if defined(LIGHTMAP_ON)
				float2 lightmapUV;
				#endif
				#if !defined(LIGHTMAP_ON)
				float3 sh;
				#endif
				float4 fogFactorAndVertexLight;
				float4 shadowCoord;
				#if UNITY_ANY_INSTANCING_ENABLED
				uint instanceID : CUSTOM_INSTANCE_ID;
				#endif
				#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
				uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
				#endif
				#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
				uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
				#endif
				#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
				FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
				#endif
			};

			// Generated Type: PackedVaryings
			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				#if defined(LIGHTMAP_ON)
				#endif
				#if !defined(LIGHTMAP_ON)
				#endif
				#if UNITY_ANY_INSTANCING_ENABLED
				uint instanceID : CUSTOM_INSTANCE_ID;
				#endif
				float3 interp00 : TEXCOORD0;
				float3 interp01 : TEXCOORD1;
				float4 interp02 : TEXCOORD2;
				float4 interp03 : TEXCOORD3;
				float3 interp04 : TEXCOORD4;
				float2 interp05 : TEXCOORD5;
				float3 interp06 : TEXCOORD6;
				float4 interp07 : TEXCOORD7;
				float4 interp08 : TEXCOORD8;
				#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
				uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
				#endif
				#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
				uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
				#endif
				#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
				FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
				#endif
			};

			// Packed Type: Varyings
			PackedVaryings PackVaryings(Varyings input)
			{
				PackedVaryings output = (PackedVaryings)0;
				output.positionCS = input.positionCS;
				output.interp00.xyz = input.positionWS;
				output.interp01.xyz = input.normalWS;
				output.interp02.xyzw = input.tangentWS;
				output.interp03.xyzw = input.texCoord0;
				output.interp04.xyz = input.viewDirectionWS;
				#if defined(LIGHTMAP_ON)
				output.interp05.xy = input.lightmapUV;
				#endif
				#if !defined(LIGHTMAP_ON)
				output.interp06.xyz = input.sh;
				#endif
				output.interp07.xyzw = input.fogFactorAndVertexLight;
				output.interp08.xyzw = input.shadowCoord;
				#if UNITY_ANY_INSTANCING_ENABLED
				output.instanceID = input.instanceID;
				#endif
				#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
				output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
				#endif
				#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
				output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
				#endif
				#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
				output.cullFace = input.cullFace;
				#endif
				return output;
			}

			// Unpacked Type: Varyings
			Varyings UnpackVaryings(PackedVaryings input)
			{
				Varyings output = (Varyings)0;
				output.positionCS = input.positionCS;
				output.positionWS = input.interp00.xyz;
				output.normalWS = input.interp01.xyz;
				output.tangentWS = input.interp02.xyzw;
				output.texCoord0 = input.interp03.xyzw;
				output.viewDirectionWS = input.interp04.xyz;
				#if defined(LIGHTMAP_ON)
				output.lightmapUV = input.interp05.xy;
				#endif
				#if !defined(LIGHTMAP_ON)
				output.sh = input.interp06.xyz;
				#endif
				output.fogFactorAndVertexLight = input.interp07.xyzw;
				output.shadowCoord = input.interp08.xyzw;
				#if UNITY_ANY_INSTANCING_ENABLED
				output.instanceID = input.instanceID;
				#endif
				#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
				output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
				#endif
				#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
				output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
				#endif
				#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
				output.cullFace = input.cullFace;
				#endif
				return output;
			}

			// --------------------------------------------------
			// Build Graph Inputs

			SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
			{
				SurfaceDescriptionInputs output;
				ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



				output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


				output.WorldSpacePosition = input.positionWS;
				output.ViewSpacePosition = TransformWorldToView(input.positionWS);
				output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
				output.uv0 = input.texCoord0;
			#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
			#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
			#else
			#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
			#endif
			#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

				return output;
			}


			// --------------------------------------------------
			// Main

			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"

			ENDHLSL
		}

		Pass
		{
			Name "ShadowCaster"
			Tags
			{
				"LightMode" = "ShadowCaster"
			}

				// Render State
				Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
				Cull Back
				ZTest LEqual
				ZWrite On
				// ColorMask: <None>


				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				// Debug
				// <None>

				// --------------------------------------------------
				// Pass

				// Pragmas
				#pragma prefer_hlslcc gles
				#pragma exclude_renderers d3d11_9x
				#pragma target 2.0
				#pragma multi_compile_instancing

				// Keywords
				// PassKeywords: <None>
				// GraphKeywords: <None>

				// Defines
				#define _SURFACE_TYPE_TRANSPARENT 1
				#define _NORMAL_DROPOFF_TS 1
				#define ATTRIBUTES_NEED_NORMAL
				#define ATTRIBUTES_NEED_TANGENT
				#pragma multi_compile_instancing
				#define SHADERPASS_SHADOWCASTER


				// Includes
				#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
				#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

				// --------------------------------------------------
				// Graph

				// Graph Properties
				CBUFFER_START(UnityPerMaterial)
				float4 Color_F65BA171;
				float Vector1_83E209B8;
				float Vector1_8BFBC7FE;
				float Vector1_B0F3CB11;
				float Vector1_5A1B61E1;
				float2 Vector2_E513D0B4;
				float2 Vector2_1ACA2B2;
				float4 Color_163D05C0;
				CBUFFER_END
				TEXTURE2D(Texture2D_118B5EB3); SAMPLER(samplerTexture2D_118B5EB3); float4 Texture2D_118B5EB3_TexelSize;
				TEXTURE2D(Texture2D_A73B07CA); SAMPLER(samplerTexture2D_A73B07CA); float4 Texture2D_A73B07CA_TexelSize;
				TEXTURE2D(Texture2D_9971ADF4); SAMPLER(samplerTexture2D_9971ADF4); float4 Texture2D_9971ADF4_TexelSize;
				TEXTURE2D(Texture2D_67E365CE); SAMPLER(samplerTexture2D_67E365CE); float4 Texture2D_67E365CE_TexelSize;

				// Graph Functions
				// GraphFunctions: <None>

				// Graph Vertex
				// GraphVertex: <None>

				// Graph Pixel
				struct SurfaceDescriptionInputs
				{
					float3 TangentSpaceNormal;
				};

				struct SurfaceDescription
				{
					float Alpha;
					float AlphaClipThreshold;
				};

				SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
				{
					SurfaceDescription surface = (SurfaceDescription)0;
					surface.Alpha = 1;
					surface.AlphaClipThreshold = 0;
					return surface;
				}

				// --------------------------------------------------
				// Structs and Packing

				// Generated Type: Attributes
				struct Attributes
				{
					float3 positionOS : POSITION;
					float3 normalOS : NORMAL;
					float4 tangentOS : TANGENT;
					#if UNITY_ANY_INSTANCING_ENABLED
					uint instanceID : INSTANCEID_SEMANTIC;
					#endif
				};

				// Generated Type: Varyings
				struct Varyings
				{
					float4 positionCS : SV_POSITION;
					#if UNITY_ANY_INSTANCING_ENABLED
					uint instanceID : CUSTOM_INSTANCE_ID;
					#endif
					#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
					uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
					#endif
					#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
					uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
					#endif
					#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
					FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
					#endif
				};

				// Generated Type: PackedVaryings
				struct PackedVaryings
				{
					float4 positionCS : SV_POSITION;
					#if UNITY_ANY_INSTANCING_ENABLED
					uint instanceID : CUSTOM_INSTANCE_ID;
					#endif
					#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
					uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
					#endif
					#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
					uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
					#endif
					#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
					FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
					#endif
				};

				// Packed Type: Varyings
				PackedVaryings PackVaryings(Varyings input)
				{
					PackedVaryings output = (PackedVaryings)0;
					output.positionCS = input.positionCS;
					#if UNITY_ANY_INSTANCING_ENABLED
					output.instanceID = input.instanceID;
					#endif
					#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
					output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
					#endif
					#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
					output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
					#endif
					#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
					output.cullFace = input.cullFace;
					#endif
					return output;
				}

				// Unpacked Type: Varyings
				Varyings UnpackVaryings(PackedVaryings input)
				{
					Varyings output = (Varyings)0;
					output.positionCS = input.positionCS;
					#if UNITY_ANY_INSTANCING_ENABLED
					output.instanceID = input.instanceID;
					#endif
					#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
					output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
					#endif
					#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
					output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
					#endif
					#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
					output.cullFace = input.cullFace;
					#endif
					return output;
				}

				// --------------------------------------------------
				// Build Graph Inputs

				SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
				{
					SurfaceDescriptionInputs output;
					ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



					output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


				#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
				#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
				#else
				#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
				#endif
				#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

					return output;
				}


				// --------------------------------------------------
				// Main

				#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

				ENDHLSL
			}

			Pass
			{
				Name "DepthOnly"
				Tags
				{
					"LightMode" = "DepthOnly"
				}

					// Render State
					Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
					Cull Back
					ZTest LEqual
					ZWrite On
					ColorMask 0


					HLSLPROGRAM
					#pragma vertex vert
					#pragma fragment frag

					// Debug
					// <None>

					// --------------------------------------------------
					// Pass

					// Pragmas
					#pragma prefer_hlslcc gles
					#pragma exclude_renderers d3d11_9x
					#pragma target 2.0
					#pragma multi_compile_instancing

					// Keywords
					// PassKeywords: <None>
					// GraphKeywords: <None>

					// Defines
					#define _SURFACE_TYPE_TRANSPARENT 1
					#define _NORMAL_DROPOFF_TS 1
					#define ATTRIBUTES_NEED_NORMAL
					#define ATTRIBUTES_NEED_TANGENT
					#pragma multi_compile_instancing
					#define SHADERPASS_DEPTHONLY


					// Includes
					#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
					#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
					#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

					// --------------------------------------------------
					// Graph

					// Graph Properties
					CBUFFER_START(UnityPerMaterial)
					float4 Color_F65BA171;
					float Vector1_83E209B8;
					float Vector1_8BFBC7FE;
					float Vector1_B0F3CB11;
					float Vector1_5A1B61E1;
					float2 Vector2_E513D0B4;
					float2 Vector2_1ACA2B2;
					float4 Color_163D05C0;
					CBUFFER_END
					TEXTURE2D(Texture2D_118B5EB3); SAMPLER(samplerTexture2D_118B5EB3); float4 Texture2D_118B5EB3_TexelSize;
					TEXTURE2D(Texture2D_A73B07CA); SAMPLER(samplerTexture2D_A73B07CA); float4 Texture2D_A73B07CA_TexelSize;
					TEXTURE2D(Texture2D_9971ADF4); SAMPLER(samplerTexture2D_9971ADF4); float4 Texture2D_9971ADF4_TexelSize;
					TEXTURE2D(Texture2D_67E365CE); SAMPLER(samplerTexture2D_67E365CE); float4 Texture2D_67E365CE_TexelSize;

					// Graph Functions
					// GraphFunctions: <None>

					// Graph Vertex
					// GraphVertex: <None>

					// Graph Pixel
					struct SurfaceDescriptionInputs
					{
						float3 TangentSpaceNormal;
					};

					struct SurfaceDescription
					{
						float Alpha;
						float AlphaClipThreshold;
					};

					SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
					{
						SurfaceDescription surface = (SurfaceDescription)0;
						surface.Alpha = 1;
						surface.AlphaClipThreshold = 0;
						return surface;
					}

					// --------------------------------------------------
					// Structs and Packing

					// Generated Type: Attributes
					struct Attributes
					{
						float3 positionOS : POSITION;
						float3 normalOS : NORMAL;
						float4 tangentOS : TANGENT;
						#if UNITY_ANY_INSTANCING_ENABLED
						uint instanceID : INSTANCEID_SEMANTIC;
						#endif
					};

					// Generated Type: Varyings
					struct Varyings
					{
						float4 positionCS : SV_POSITION;
						#if UNITY_ANY_INSTANCING_ENABLED
						uint instanceID : CUSTOM_INSTANCE_ID;
						#endif
						#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
						uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
						#endif
						#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
						uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
						#endif
						#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
						FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
						#endif
					};

					// Generated Type: PackedVaryings
					struct PackedVaryings
					{
						float4 positionCS : SV_POSITION;
						#if UNITY_ANY_INSTANCING_ENABLED
						uint instanceID : CUSTOM_INSTANCE_ID;
						#endif
						#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
						uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
						#endif
						#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
						uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
						#endif
						#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
						FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
						#endif
					};

					// Packed Type: Varyings
					PackedVaryings PackVaryings(Varyings input)
					{
						PackedVaryings output = (PackedVaryings)0;
						output.positionCS = input.positionCS;
						#if UNITY_ANY_INSTANCING_ENABLED
						output.instanceID = input.instanceID;
						#endif
						#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
						output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
						#endif
						#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
						output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
						#endif
						#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
						output.cullFace = input.cullFace;
						#endif
						return output;
					}

					// Unpacked Type: Varyings
					Varyings UnpackVaryings(PackedVaryings input)
					{
						Varyings output = (Varyings)0;
						output.positionCS = input.positionCS;
						#if UNITY_ANY_INSTANCING_ENABLED
						output.instanceID = input.instanceID;
						#endif
						#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
						output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
						#endif
						#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
						output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
						#endif
						#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
						output.cullFace = input.cullFace;
						#endif
						return output;
					}

					// --------------------------------------------------
					// Build Graph Inputs

					SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
					{
						SurfaceDescriptionInputs output;
						ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



						output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


					#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
					#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
					#else
					#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
					#endif
					#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

						return output;
					}


					// --------------------------------------------------
					// Main

					#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
					#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

					ENDHLSL
				}

				Pass
				{
					Name "Meta"
					Tags
					{
						"LightMode" = "Meta"
					}

						// Render State
						Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
						Cull Back
						ZTest LEqual
						ZWrite On
						// ColorMask: <None>


						HLSLPROGRAM
						#pragma vertex vert
						#pragma fragment frag

						// Debug
						// <None>

						// --------------------------------------------------
						// Pass

						// Pragmas
						#pragma prefer_hlslcc gles
						#pragma exclude_renderers d3d11_9x
						#pragma target 2.0

						// Keywords
						#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
						// GraphKeywords: <None>

						// Defines
						#define _SURFACE_TYPE_TRANSPARENT 1
						#define _NORMAL_DROPOFF_TS 1
						#define ATTRIBUTES_NEED_NORMAL
						#define ATTRIBUTES_NEED_TANGENT
						#define ATTRIBUTES_NEED_TEXCOORD0
						#define ATTRIBUTES_NEED_TEXCOORD1
						#define ATTRIBUTES_NEED_TEXCOORD2
						#define VARYINGS_NEED_POSITION_WS 
						#define VARYINGS_NEED_TEXCOORD0
						#pragma multi_compile_instancing
						#define SHADERPASS_META
						#define REQUIRE_DEPTH_TEXTURE


						// Includes
						#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
						#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
						#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
						#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
						#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
						#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

						// --------------------------------------------------
						// Graph

						// Graph Properties
						CBUFFER_START(UnityPerMaterial)
						float4 Color_F65BA171;
						float Vector1_83E209B8;
						float Vector1_8BFBC7FE;
						float Vector1_B0F3CB11;
						float Vector1_5A1B61E1;
						float2 Vector2_E513D0B4;
						float2 Vector2_1ACA2B2;
						float4 Color_163D05C0;
						CBUFFER_END
						TEXTURE2D(Texture2D_118B5EB3); SAMPLER(samplerTexture2D_118B5EB3); float4 Texture2D_118B5EB3_TexelSize;
						TEXTURE2D(Texture2D_A73B07CA); SAMPLER(samplerTexture2D_A73B07CA); float4 Texture2D_A73B07CA_TexelSize;
						TEXTURE2D(Texture2D_9971ADF4); SAMPLER(samplerTexture2D_9971ADF4); float4 Texture2D_9971ADF4_TexelSize;
						TEXTURE2D(Texture2D_67E365CE); SAMPLER(samplerTexture2D_67E365CE); float4 Texture2D_67E365CE_TexelSize;
						SAMPLER(_SampleTexture2D_5A082E38_Sampler_3_Linear_Repeat);

						// Graph Functions

						void Unity_Negate_float(float In, out float Out)
						{
							Out = -1 * In;
						}

						void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
						{
							Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
						}

						void Unity_Comparison_LessOrEqual_float(float A, float B, out float Out)
						{
							Out = A <= B ? 1 : 0;
						}

						void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
						{
							Out = UV * Tiling + Offset;
						}

						void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
						{
							Out = A * B;
						}

						void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
						{
							Out = Predicate ? True : False;
						}

						// Graph Vertex
						// GraphVertex: <None>

						// Graph Pixel
						struct SurfaceDescriptionInputs
						{
							float3 TangentSpaceNormal;
							float3 ViewSpacePosition;
							float3 WorldSpacePosition;
							float4 ScreenPosition;
							float4 uv0;
						};

						struct SurfaceDescription
						{
							float3 Albedo;
							float3 Emission;
							float Alpha;
							float AlphaClipThreshold;
						};

						SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
						{
							SurfaceDescription surface = (SurfaceDescription)0;
							float _Split_5525744F_R_1 = IN.ViewSpacePosition[0];
							float _Split_5525744F_G_2 = IN.ViewSpacePosition[1];
							float _Split_5525744F_B_3 = IN.ViewSpacePosition[2];
							float _Split_5525744F_A_4 = 0;
							float _Negate_280F0620_Out_1;
							Unity_Negate_float(_Split_5525744F_B_3, _Negate_280F0620_Out_1);
							float _SceneDepth_3EBB0BC0_Out_1;
							Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3EBB0BC0_Out_1);
							float _Comparison_8FF820DD_Out_2;
							Unity_Comparison_LessOrEqual_float(_Negate_280F0620_Out_1, _SceneDepth_3EBB0BC0_Out_1, _Comparison_8FF820DD_Out_2);
							float4 _Property_379B64D3_Out_0 = Color_F65BA171;
							float2 _Property_F46A7F06_Out_0 = Vector2_1ACA2B2;
							float2 _Property_693B57AB_Out_0 = Vector2_E513D0B4;
							float2 _TilingAndOffset_A5689075_Out_3;
							Unity_TilingAndOffset_float(IN.uv0.xy, _Property_F46A7F06_Out_0, _Property_693B57AB_Out_0, _TilingAndOffset_A5689075_Out_3);
							float4 _SampleTexture2D_5A082E38_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_118B5EB3, samplerTexture2D_118B5EB3, _TilingAndOffset_A5689075_Out_3);
							float _SampleTexture2D_5A082E38_R_4 = _SampleTexture2D_5A082E38_RGBA_0.r;
							float _SampleTexture2D_5A082E38_G_5 = _SampleTexture2D_5A082E38_RGBA_0.g;
							float _SampleTexture2D_5A082E38_B_6 = _SampleTexture2D_5A082E38_RGBA_0.b;
							float _SampleTexture2D_5A082E38_A_7 = _SampleTexture2D_5A082E38_RGBA_0.a;
							float4 _Multiply_FDC78828_Out_2;
							Unity_Multiply_float(_Property_379B64D3_Out_0, _SampleTexture2D_5A082E38_RGBA_0, _Multiply_FDC78828_Out_2);
							float4 _Property_6B9A620A_Out_0 = Color_163D05C0;
							float4 _Branch_7A8C5EB7_Out_3;
							Unity_Branch_float4(_Comparison_8FF820DD_Out_2, _Multiply_FDC78828_Out_2, _Property_6B9A620A_Out_0, _Branch_7A8C5EB7_Out_3);
							surface.Albedo = (_Branch_7A8C5EB7_Out_3.xyz);
							surface.Emission = IsGammaSpace() ? float3(0, 0, 0) : SRGBToLinear(float3(0, 0, 0));
							surface.Alpha = 1;
							surface.AlphaClipThreshold = 0;
							return surface;
						}

						// --------------------------------------------------
						// Structs and Packing

						// Generated Type: Attributes
						struct Attributes
						{
							float3 positionOS : POSITION;
							float3 normalOS : NORMAL;
							float4 tangentOS : TANGENT;
							float4 uv0 : TEXCOORD0;
							float4 uv1 : TEXCOORD1;
							float4 uv2 : TEXCOORD2;
							#if UNITY_ANY_INSTANCING_ENABLED
							uint instanceID : INSTANCEID_SEMANTIC;
							#endif
						};

						// Generated Type: Varyings
						struct Varyings
						{
							float4 positionCS : SV_POSITION;
							float3 positionWS;
							float4 texCoord0;
							#if UNITY_ANY_INSTANCING_ENABLED
							uint instanceID : CUSTOM_INSTANCE_ID;
							#endif
							#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
							uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
							#endif
							#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
							uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
							#endif
							#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
							FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
							#endif
						};

						// Generated Type: PackedVaryings
						struct PackedVaryings
						{
							float4 positionCS : SV_POSITION;
							#if UNITY_ANY_INSTANCING_ENABLED
							uint instanceID : CUSTOM_INSTANCE_ID;
							#endif
							float3 interp00 : TEXCOORD0;
							float4 interp01 : TEXCOORD1;
							#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
							uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
							#endif
							#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
							uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
							#endif
							#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
							FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
							#endif
						};

						// Packed Type: Varyings
						PackedVaryings PackVaryings(Varyings input)
						{
							PackedVaryings output = (PackedVaryings)0;
							output.positionCS = input.positionCS;
							output.interp00.xyz = input.positionWS;
							output.interp01.xyzw = input.texCoord0;
							#if UNITY_ANY_INSTANCING_ENABLED
							output.instanceID = input.instanceID;
							#endif
							#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
							output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
							#endif
							#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
							output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
							#endif
							#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
							output.cullFace = input.cullFace;
							#endif
							return output;
						}

						// Unpacked Type: Varyings
						Varyings UnpackVaryings(PackedVaryings input)
						{
							Varyings output = (Varyings)0;
							output.positionCS = input.positionCS;
							output.positionWS = input.interp00.xyz;
							output.texCoord0 = input.interp01.xyzw;
							#if UNITY_ANY_INSTANCING_ENABLED
							output.instanceID = input.instanceID;
							#endif
							#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
							output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
							#endif
							#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
							output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
							#endif
							#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
							output.cullFace = input.cullFace;
							#endif
							return output;
						}

						// --------------------------------------------------
						// Build Graph Inputs

						SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
						{
							SurfaceDescriptionInputs output;
							ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



							output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


							output.WorldSpacePosition = input.positionWS;
							output.ViewSpacePosition = TransformWorldToView(input.positionWS);
							output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
							output.uv0 = input.texCoord0;
						#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
						#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
						#else
						#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
						#endif
						#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

							return output;
						}


						// --------------------------------------------------
						// Main

						#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
						#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"

						ENDHLSL
					}

					Pass
					{
							// Name: <None>
							Tags
							{
								"LightMode" = "Universal2D"
							}

							// Render State
							Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
							Cull Back
							ZTest LEqual
							ZWrite Off
							// ColorMask: <None>


							HLSLPROGRAM
							#pragma vertex vert
							#pragma fragment frag

							// Debug
							// <None>

							// --------------------------------------------------
							// Pass

							// Pragmas
							#pragma prefer_hlslcc gles
							#pragma exclude_renderers d3d11_9x
							#pragma target 2.0
							#pragma multi_compile_instancing

							// Keywords
							// PassKeywords: <None>
							// GraphKeywords: <None>

							// Defines
							#define _SURFACE_TYPE_TRANSPARENT 1
							#define _NORMAL_DROPOFF_TS 1
							#define ATTRIBUTES_NEED_NORMAL
							#define ATTRIBUTES_NEED_TANGENT
							#define ATTRIBUTES_NEED_TEXCOORD0
							#define VARYINGS_NEED_POSITION_WS 
							#define VARYINGS_NEED_TEXCOORD0
							#pragma multi_compile_instancing
							#define SHADERPASS_2D
							#define REQUIRE_DEPTH_TEXTURE


							// Includes
							#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
							#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
							#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
							#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
							#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

							// --------------------------------------------------
							// Graph

							// Graph Properties
							CBUFFER_START(UnityPerMaterial)
							float4 Color_F65BA171;
							float Vector1_83E209B8;
							float Vector1_8BFBC7FE;
							float Vector1_B0F3CB11;
							float Vector1_5A1B61E1;
							float2 Vector2_E513D0B4;
							float2 Vector2_1ACA2B2;
							float4 Color_163D05C0;
							CBUFFER_END
							TEXTURE2D(Texture2D_118B5EB3); SAMPLER(samplerTexture2D_118B5EB3); float4 Texture2D_118B5EB3_TexelSize;
							TEXTURE2D(Texture2D_A73B07CA); SAMPLER(samplerTexture2D_A73B07CA); float4 Texture2D_A73B07CA_TexelSize;
							TEXTURE2D(Texture2D_9971ADF4); SAMPLER(samplerTexture2D_9971ADF4); float4 Texture2D_9971ADF4_TexelSize;
							TEXTURE2D(Texture2D_67E365CE); SAMPLER(samplerTexture2D_67E365CE); float4 Texture2D_67E365CE_TexelSize;
							SAMPLER(_SampleTexture2D_5A082E38_Sampler_3_Linear_Repeat);

							// Graph Functions

							void Unity_Negate_float(float In, out float Out)
							{
								Out = -1 * In;
							}

							void Unity_SceneDepth_Eye_float(float4 UV, out float Out)
							{
								Out = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy), _ZBufferParams);
							}

							void Unity_Comparison_LessOrEqual_float(float A, float B, out float Out)
							{
								Out = A <= B ? 1 : 0;
							}

							void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
							{
								Out = UV * Tiling + Offset;
							}

							void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
							{
								Out = A * B;
							}

							void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
							{
								Out = Predicate ? True : False;
							}

							// Graph Vertex
							// GraphVertex: <None>

							// Graph Pixel
							struct SurfaceDescriptionInputs
							{
								float3 TangentSpaceNormal;
								float3 ViewSpacePosition;
								float3 WorldSpacePosition;
								float4 ScreenPosition;
								float4 uv0;
							};

							struct SurfaceDescription
							{
								float3 Albedo;
								float Alpha;
								float AlphaClipThreshold;
							};

							SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
							{
								SurfaceDescription surface = (SurfaceDescription)0;
								float _Split_5525744F_R_1 = IN.ViewSpacePosition[0];
								float _Split_5525744F_G_2 = IN.ViewSpacePosition[1];
								float _Split_5525744F_B_3 = IN.ViewSpacePosition[2];
								float _Split_5525744F_A_4 = 0;
								float _Negate_280F0620_Out_1;
								Unity_Negate_float(_Split_5525744F_B_3, _Negate_280F0620_Out_1);
								float _SceneDepth_3EBB0BC0_Out_1;
								Unity_SceneDepth_Eye_float(float4(IN.ScreenPosition.xy / IN.ScreenPosition.w, 0, 0), _SceneDepth_3EBB0BC0_Out_1);
								float _Comparison_8FF820DD_Out_2;
								Unity_Comparison_LessOrEqual_float(_Negate_280F0620_Out_1, _SceneDepth_3EBB0BC0_Out_1, _Comparison_8FF820DD_Out_2);
								float4 _Property_379B64D3_Out_0 = Color_F65BA171;
								float2 _Property_F46A7F06_Out_0 = Vector2_1ACA2B2;
								float2 _Property_693B57AB_Out_0 = Vector2_E513D0B4;
								float2 _TilingAndOffset_A5689075_Out_3;
								Unity_TilingAndOffset_float(IN.uv0.xy, _Property_F46A7F06_Out_0, _Property_693B57AB_Out_0, _TilingAndOffset_A5689075_Out_3);
								float4 _SampleTexture2D_5A082E38_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_118B5EB3, samplerTexture2D_118B5EB3, _TilingAndOffset_A5689075_Out_3);
								float _SampleTexture2D_5A082E38_R_4 = _SampleTexture2D_5A082E38_RGBA_0.r;
								float _SampleTexture2D_5A082E38_G_5 = _SampleTexture2D_5A082E38_RGBA_0.g;
								float _SampleTexture2D_5A082E38_B_6 = _SampleTexture2D_5A082E38_RGBA_0.b;
								float _SampleTexture2D_5A082E38_A_7 = _SampleTexture2D_5A082E38_RGBA_0.a;
								float4 _Multiply_FDC78828_Out_2;
								Unity_Multiply_float(_Property_379B64D3_Out_0, _SampleTexture2D_5A082E38_RGBA_0, _Multiply_FDC78828_Out_2);
								float4 _Property_6B9A620A_Out_0 = Color_163D05C0;
								float4 _Branch_7A8C5EB7_Out_3;
								Unity_Branch_float4(_Comparison_8FF820DD_Out_2, _Multiply_FDC78828_Out_2, _Property_6B9A620A_Out_0, _Branch_7A8C5EB7_Out_3);
								surface.Albedo = (_Branch_7A8C5EB7_Out_3.xyz);
								surface.Alpha = 1;
								surface.AlphaClipThreshold = 0;
								return surface;
							}

							// --------------------------------------------------
							// Structs and Packing

							// Generated Type: Attributes
							struct Attributes
							{
								float3 positionOS : POSITION;
								float3 normalOS : NORMAL;
								float4 tangentOS : TANGENT;
								float4 uv0 : TEXCOORD0;
								#if UNITY_ANY_INSTANCING_ENABLED
								uint instanceID : INSTANCEID_SEMANTIC;
								#endif
							};

							// Generated Type: Varyings
							struct Varyings
							{
								float4 positionCS : SV_POSITION;
								float3 positionWS;
								float4 texCoord0;
								#if UNITY_ANY_INSTANCING_ENABLED
								uint instanceID : CUSTOM_INSTANCE_ID;
								#endif
								#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
								uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
								#endif
								#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
								uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
								#endif
								#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
								FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
								#endif
							};

							// Generated Type: PackedVaryings
							struct PackedVaryings
							{
								float4 positionCS : SV_POSITION;
								#if UNITY_ANY_INSTANCING_ENABLED
								uint instanceID : CUSTOM_INSTANCE_ID;
								#endif
								float3 interp00 : TEXCOORD0;
								float4 interp01 : TEXCOORD1;
								#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
								uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
								#endif
								#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
								uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
								#endif
								#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
								FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
								#endif
							};

							// Packed Type: Varyings
							PackedVaryings PackVaryings(Varyings input)
							{
								PackedVaryings output = (PackedVaryings)0;
								output.positionCS = input.positionCS;
								output.interp00.xyz = input.positionWS;
								output.interp01.xyzw = input.texCoord0;
								#if UNITY_ANY_INSTANCING_ENABLED
								output.instanceID = input.instanceID;
								#endif
								#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
								output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
								#endif
								#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
								output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
								#endif
								#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
								output.cullFace = input.cullFace;
								#endif
								return output;
							}

							// Unpacked Type: Varyings
							Varyings UnpackVaryings(PackedVaryings input)
							{
								Varyings output = (Varyings)0;
								output.positionCS = input.positionCS;
								output.positionWS = input.interp00.xyz;
								output.texCoord0 = input.interp01.xyzw;
								#if UNITY_ANY_INSTANCING_ENABLED
								output.instanceID = input.instanceID;
								#endif
								#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
								output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
								#endif
								#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
								output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
								#endif
								#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
								output.cullFace = input.cullFace;
								#endif
								return output;
							}

							// --------------------------------------------------
							// Build Graph Inputs

							SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
							{
								SurfaceDescriptionInputs output;
								ZERO_INITIALIZE(SurfaceDescriptionInputs, output);



								output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);


								output.WorldSpacePosition = input.positionWS;
								output.ViewSpacePosition = TransformWorldToView(input.positionWS);
								output.ScreenPosition = ComputeScreenPos(TransformWorldToHClip(input.positionWS), _ProjectionParams.x);
								output.uv0 = input.texCoord0;
							#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
							#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
							#else
							#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
							#endif
							#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

								return output;
							}


							// --------------------------------------------------
							// Main

							#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
							#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"

							ENDHLSL
						}

		}
			CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
								FallBack "Hidden/Shader Graph/FallbackError"
}
