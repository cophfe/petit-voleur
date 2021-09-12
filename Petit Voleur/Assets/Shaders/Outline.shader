Shader "Custom/Unlit/Outline"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size", Float) = 0
    }
    SubShader
    {

        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		Cull False

		Pass
		{
			CGPROGRAM
			// make fog work
			#pragma multi_compile_fog
			//dependancies
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _Color;
			half _OutlineSize;

			v2f vert(appdata v)
			{

				v2f o;
				//https://alexanderameye.github.io/notes/rendering-outlines/ << explains how to make _OutlineSize in pixel units

				o.vertex = UnityObjectToClipPos(v.vertex);
				//put normal in clip space
				float3 normal = mul((float3x3)UNITY_MATRIX_VP, mul((float3x3)UNITY_MATRIX_M, v.normal));
				//move vertex along normal
				o.vertex.xy += normalize(normal.xy) / _ScreenParams.xy * o.vertex.w * _OutlineSize * 2;

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag(v2f i, fixed facing : VFACE) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
    }
}
