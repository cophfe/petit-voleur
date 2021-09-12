/*==================================================
	Programmer: Connor Fettes
==================================================*/
//image effect shader that has box blur effect

Shader "Custom/Blur"
{
	//Box Blur
    Properties
    {
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
		_BlurAmount("Blur Amount", Range(0,0.1)) = 1
		[IntRange]_BlurSamples("Blur Samples", Range(1,40)) = 10
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _BlurAmount;
			int _BlurSamples;

			fixed4 frag(v2f i) : SV_Target
			{
				float aspect = _ScreenParams.x / _ScreenParams.y;
				fixed4 col = 0;

				for (float index = 0; index < _BlurSamples; index++)
				{
					//get uv coordinate of sample
					float2 uv = i.uv + float2(0, aspect *(index / (_BlurSamples - 1) - 0.5) * _BlurAmount);
					//add color at position to color
					col += tex2D(_MainTex, uv);
				}
                return col/ _BlurSamples;
            }
            ENDCG
        }
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
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			float _BlurAmount;
			int _BlurSamples;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = 0;

				for (float index = 0; index < _BlurSamples; index++)
				{
					//get uv coordinate of sample
					float2 uv = i.uv + float2((index / (_BlurSamples - 1) - 0.5) * _BlurAmount, 0);
					//add color at position to color
					col += tex2D(_MainTex, uv);
				}
				return col / _BlurSamples;
			}
			ENDCG
		}
    }
}
