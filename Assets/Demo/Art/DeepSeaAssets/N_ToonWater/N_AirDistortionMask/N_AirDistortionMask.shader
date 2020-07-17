// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "nreal/N_AirDistortionMask" {

	Properties {
		_BumpTex ("Bump (RGB)", 2D) = "white" {}
		_MaskTex ("Mask (RGB)", 2D) = "white" {}
		_Distortion ("Distortion", Range (0.01, 0.1)) = 0.02
		_UOffset ("U Offset", Range (-6, 6)) = 0
		_VOffset ("V Offset", Range (-6, 6)) = 0
	}
	
	
	SubShader {
		// We must be transparent, so other objects are drawn before this one.
		Tags {"Queue"="Overlay-8" "IgnoreProjector"="True" "RenderType"="Transparent"}
		
		LOD 200
		
		ZWrite Off
		
		
		GrabPass {"_Distort_GrabTexture"}
	
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord: TEXCOORD0;
			};
			
			struct v2f {
				float4 vertex : POSITION;
				float4 uvgrab : TEXCOORD0;
				float2 uvmain : TEXCOORD2;
				float2 uvmask : TEXCOORD3;
			};
			
			float _Distortion;
			float _UOffset;
			float _VOffset;
			sampler2D _Distort_GrabTexture;
			sampler2D _BumpTex;
			sampler2D _MaskTex;
			float4 _BumpTex_ST;
			float4 _MaskTex_ST;


			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
				o.uvgrab.zw = o.vertex.zw;
				o.uvmain = TRANSFORM_TEX( v.texcoord, _BumpTex );
				o.uvmask = TRANSFORM_TEX( v.texcoord, _MaskTex );
				return o;
			}
			
			
			half4 frag( v2f i ) : COLOR
			{
				half2 mask = tex2D( _MaskTex, i.uvmask );
				half2 bump = tex2D( _BumpTex, i.uvmain + float2(_UOffset,_VOffset) * fmod(_Time.y,64) ).rg;
				bump*=mask;

				i.uvgrab.xy /= i.uvgrab.w;

				half2 uvofs = bump*_Distortion;
				if (_ProjectionParams.x<0) uvofs.y = -uvofs.y;
				i.uvgrab.xy += uvofs;
				
				half4 col = tex2D( _Distort_GrabTexture, i.uvgrab.xy );
				//half4 col = tex2Dproj( _Distort_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));

				//col.rgb = bump.r;

				return col;
			}
			ENDCG
			
		} // of pass
		
	} // of subshader
	
	
	SubShader {
		Pass {
			ZWrite Off
			Lighting Off
		    Blend Zero One
		    SetTexture [_BumpTex] { combine texture }
		}
	}
	
}
