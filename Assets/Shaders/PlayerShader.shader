﻿Shader "Custom/PlayerShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_FirstTargetTex("FirstTargetTexture", 2D) = "white" {}
		_SecondTargetTex("SecondTargetTexture", 2D) = "white" {}
		_FirstColor("FirstColor", Color) = ( 1,1,1,1 )
		_SecondColor("SecondColor", Color) = ( 1,1,1,1 )
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _FirstTargetTex;
			sampler2D _SecondTargetTex;

			fixed4 _FirstColor;
			fixed4 _SecondColor;

			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 first = tex2D(_FirstTargetTex, i.uv);
				fixed4 second = tex2D(_SecondTargetTex, i.uv);
				if (first[0] == 1)
				{
					col = _FirstColor;
				}
				else if (second[0] == 1)
				{
					col = _SecondColor;
				}
				// apply fog
				return col;
			}
			ENDCG
		}
	}
}
