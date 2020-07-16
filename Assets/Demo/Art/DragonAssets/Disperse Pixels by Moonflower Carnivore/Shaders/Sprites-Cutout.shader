Shader "Sprites/Cutout"
{
	Properties {
		[PerRendererData] _MainTex ("Main texture", 2D) = "white" {}
		_DissolveTex ("Dissolution texture", 2D) = "white" {}
		_Cutoff ("Red channel cutoff", Range(0.0, 1.01)) = 0
	}

	SubShader {

		Tags { "Queue"="Transparent" "PreviewType"="Plane"}
		Cull Off Lighting Off ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma instancing_options

			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
			};
			
			sampler2D _MainTex;
			sampler2D _DissolveTex;
			float4 _DissolveTex_ST;
			float _Cutoff;

			v2f vert(appdata_base v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv.xy = v.texcoord;
				o.uv.zw = TRANSFORM_TEX(v.texcoord,_DissolveTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				float4 c = tex2D(_MainTex, i.uv.xy);
				float val = tex2D(_DissolveTex, i.uv.zw).r;

				c.a *= step(_Cutoff, val);
				return c;
			}
			ENDCG
		}
	}
}