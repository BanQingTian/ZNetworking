Shader "GameWeapon/VoltaGunTransparent" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 1.0
		_Metallic ("Metallic", 2D) = "white" {}
		_NormalTex("Normal", 2D) = "bump" {}
		_NormalIntensity("Normal Map Intensity", Range(0,2)) = 1
		_Emission("Emission", 2D) = "white" {}
		_TransValue("Transparent Value",range(0,0.8)) = 0.8
	}
	SubShader {
		Tags { "RenderType" = "Transparent" "queue" = "Transparent"}

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha:blend

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Metallic;
		sampler2D _NormalTex;
		sampler2D _Emission;
		half	  _TransValue;

		struct Input {
			float2 uv_MainTex;
			float2 uv2_NormalTex;
			float2 uv3_MetalGloss;
			float2 uv4_Emission;
		};

		half _Glossiness;
		//half _Metallic;
		fixed4 _Color;
		float _NormalIntensity;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			fixed4 metal = tex2D (_Metallic, IN.uv3_MetalGloss);
			float4 emission = tex2D(_Emission, IN.uv_MainTex);
			fixed3 normal = UnpackNormal(tex2D(_NormalTex, IN.uv2_NormalTex));
			normal = float3(normal.x * _NormalIntensity, normal.y * _NormalIntensity, normal.z);

			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = metal.rgb;
			o.Smoothness = metal.a*_Glossiness;
			o.Normal = normal;
			//o.Emission = emission.rgb;
			o.Alpha = _TransValue;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
