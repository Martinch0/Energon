Shader "Custom/ElectricBallShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_HighlightColor("HighLight", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "defaulttexture" {}
		_NoiseTex("NoiseTex", 2D) = "defaulttexture" {}
	}
		SubShader{
			Pass{
				Tags{ "LightMode" = "ForwardBase" }
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0

				fixed4 _Color;
				fixed4 _HighlightColor;
				sampler2D _MainTex;
				sampler2D _NoiseTex;
				float4 _MainTex_ST;

				struct vertexInput {
					float4 vertex : POSITION;
					float4 texCoord : TEXCOORD0;
				};

				struct vertexOutput {
					float4 pos : SV_POSITION;
					float4 tex : TEXCOORD0;
				};

				struct Input {
					float4 color : COLOR;
				};

				vertexOutput vert(vertexInput v) {
					vertexOutput o;

					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					o.tex = v.texCoord;

					return o;
				}

				float2x2 makem2(float theta) {
					float c = cos(theta);
					float s = sin(theta);
					return float2x2(c, s, -s, c);
				}

				float noise(float2 xy)
				{
					return tex2D(_NoiseTex, xy*0.025).x;
				}

				float fbm(float2 p)
				{
					float z = 2;
					float rz = 0;
					float2 bp = p;
					for (float i = 1; i < 6; i += 1)
					{
						rz += abs((noise(p) - 0.5) * 2) / z;
						z = z * 2;
						p = p * 2;
					}
					return rz;
				}

				float dualfbm(float2 p)
				{
					float2 r = p * 0.7;

					float time = _Time * 1.5;

					float2 basis = float2(fbm(r - time*10), fbm(r + time*7));
					basis = (basis - 0.5) * 2.2;
					p += basis;

					return fbm(mul(p, makem2(time * 0.2)));
				}

				float applyHighlight(float2 pos)
				{
					//float x = pow(abs(pos.x) * 2, 3);
					//float y = pow(abs(pos.y) * 2, 3);
					return pow(sqrt(pos.x*pos.x + pos.y*pos.y)*2,10);
				}

				float4 frag(vertexOutput i) : COLOR
				{
					float2 pos = i.tex.xy - 0.5;
					
					float rz = dualfbm(pos);

					float4 col = float4(float3(0.1, 0.1, 0.1) / rz, 1.0) + _HighlightColor * applyHighlight(pos);

					return _Color * col.x;
				}
					ENDCG
				}
	}
	FallBack "Diffuse"
}
