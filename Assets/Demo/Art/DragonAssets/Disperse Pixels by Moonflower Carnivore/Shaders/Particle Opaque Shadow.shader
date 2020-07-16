Shader "Particles/Opaque Shadow Surface" {
SubShader {
	Tags { "RenderType"="Opaque" "Queue"="Geometry" "PreviewType"="Plane"}
	LOD 150
	//Lighting Off

CGPROGRAM
//#pragma surface surf Lambert noforwardadd
#pragma surface surf NoLighting noforwardadd

fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten) {
	return fixed4(0,0,0,1);
}

//sampler2D _MainTex;
//sampler2D _BumpMap;

struct Input {
	float2 uv_MainTex;
	//float2 uv_BumpMap;
	float4 color : COLOR;
};

void surf (Input IN, inout SurfaceOutput o) {
	//fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
	//o.Albedo = fixed4(0,0,0,1);
	o.Albedo = fixed4(0,0,0,1);
	//o.Emission = c.rgba;
	o.Emission = IN.color;
	//o.Alpha = c.a;
	o.Alpha = IN.color.a;
	//o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG
}

Fallback "Mobile/VertexLit"
}