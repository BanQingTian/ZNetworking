Shader "nreal/MatrixFX"
{
	Properties
	{
		_MaskAll("MaskAll", 2D) = "white" {}
		_TintColor("Main Color",color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		cull off
		blend one one
			zwrite off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

		int _UseScreenAspectRatio;

		float2 screen_aspect(float2 uv)
		{
			if (_UseScreenAspectRatio == 0)
				return uv;

			uv.x -= 0.5;
			uv.x *= _ScreenParams.x / _ScreenParams.y;
			uv.x += 0.5;
			return uv;
		}

			float random(in float x) {
				return frac(sin(x)*43758.5453);
			}

			float random(in float2 st) {
				return frac(sin(dot(st.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			float randomChar(in float2 outer,in float2 inner) {
				float grid = 15;
				float2 margin = float2(0.2,.05);
				float2 seed = 20.;
				float2 borders = step(margin,inner)*step(margin,1 - inner);
				return step(0.6,random(outer*seed + floor(inner*grid))) * borders.x * borders.y;
			}

			float3 matrixAll(in float2 st) {
				float rows = 150.0;
				float2 ipos = floor(st*rows) + float2(1.,0.);

				ipos += float2(.0,floor(_Time.y*20.*random(ipos.x)));

				float2 fpos = frac(st*rows);
				float2 center = (.5 - fpos);

				float pct = random(ipos);
				float glow = (1. - dot(center,center)*3.)*.5;

				return float3(randomChar(ipos, fpos) * pct * glow, randomChar(ipos, fpos) * pct * glow, randomChar(ipos, fpos) * pct * glow);
			}


			//void mainImage(out vec4 fragColor, in vec2 fragCoord) {
			//	vec2 st = fragCoord.xy / iResolution.xy;
			//	st.y *= iResolution.y / iResolution.x;

			//	fragColor = vec4(matrix(st),1.0);
			//}

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

			sampler2D _MaskAll;
			float4 _MaskAll_ST;
			fixed4 _TintColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MaskAll);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				i.uv = screen_aspect(i.uv);
				// sample the texture
				fixed4 maskCol = tex2D(_MaskAll, i.uv);
				float2 xx = matrixAll(i.uv);
				fixed4 col = float4(xx, xx.x, 1)*_TintColor * maskCol;
				return col;
			}
			ENDCG
		}
	}
}
