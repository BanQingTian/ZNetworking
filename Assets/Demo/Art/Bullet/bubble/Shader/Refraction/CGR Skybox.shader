// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Crystal Glass/Refraction/Skybox" {
	Properties {
		_CubeTex ("Environment Map", Cube) = "white" {}
	}
	CGINCLUDE
		#include "UnityCG.cginc"
		uniform samplerCUBE _CubeTex;
		struct v2f
		{
			float4 pos : SV_POSITION;
			float3 view : TEXCOORD0;
		};
		v2f vert (appdata_base v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
            o.view = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
			return o;
       	}
       	float4 frag (v2f i) : SV_TARGET
		{
			return float4(texCUBE(_CubeTex, i.view).rgb, 1);
		}
	ENDCG
	SubShader {
		Tags { "Queue" = "Background" }
		Pass {
			ZWrite Off
			Cull Front
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
	FallBack Off
}