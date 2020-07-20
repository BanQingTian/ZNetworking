// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Crystal Glass/Refraction/Mesh" {
	Properties {
		_SceneTex ("Scene Tex", 2D) = "white" {}
		_BumpTex ("Bump Tex", 2D) = "black" {}
		_BumpScale ("Bump Scale", Range(0, 0.3)) = 0.15
		_TintColor ("Glass Color", Color) = (1, 1, 1, 1)
	}
	CGINCLUDE
		#include "UnityCG.cginc"
		uniform sampler2D _SceneTex, _BumpTex;
		uniform float4 _BumpTex_ST;
		uniform float _BumpScale;
		uniform float4 _TintColor;
		struct v2f
		{
			float4 pos : POSITION;
			float2 tex : TEXCOORD0;
			float4 scrpos : TEXCOORD1;
			float3 norm : TEXCOORD2;
		};
		v2f vert (appdata_base v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.tex = TRANSFORM_TEX(v.texcoord, _BumpTex);
			o.scrpos = ComputeScreenPos(o.pos);
			o.norm = mul((float3x3)UNITY_MATRIX_IT_MV, SCALED_NORMAL);
			return o;
       	}
       	float4 frag (v2f i) : COLOR
		{
			float2 uv = i.scrpos.xy / i.scrpos.w;
#if CRYSTAL_GLASS_BUMP
			float2 bump = 2 * tex2D(_BumpTex, i.tex).xy - 1;
#else
			float2 bump = normalize(i.norm.xy);
#endif
			float4 refrA = tex2D(_SceneTex, uv + bump * _BumpScale);
			float4 refrB = tex2D(_SceneTex, uv);
			return refrB * refrB.a + refrA * (1 - refrB.a) * _TintColor;
		}
	ENDCG
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ CRYSTAL_GLASS_BUMP
			ENDCG
		}
	}
	FallBack Off
}