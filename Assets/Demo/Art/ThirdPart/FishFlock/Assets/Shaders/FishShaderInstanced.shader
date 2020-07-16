Shader "FishFlock/FishShaderInstanced" 
{
   Properties 
   {
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_AnimationSpeed("Animation Speed", Range(0 , 10)) = 0.5
		_Scale("Scale", Range(0 , 1)) = 0.8
		_Yaw("Yaw", Float) = 0.2
		_Roll("Roll", Float) = 0.2
	}

	SubShader 
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry+0" }

		CGPROGRAM

		struct Input
		{
			float2 uvcoord;
		};

		sampler2D _MainTex;
		uniform float _AnimationSpeed;
		uniform float _Yaw;
		uniform float _Roll;
		uniform float _Scale;

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		#pragma multi_compile_instancing
        #pragma surface surf Standard vertex:vert addshadow nolightmap
        #pragma instancing_options procedural:setup
		#pragma target 3.0

        float4x4 lookAtMatrix;
        float3 fishPosition;

         #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            struct FishBehaviourGPU
            {
                float3 position;
                float3 velocity;
				float speed;
				float rot_speed;
                float speed_offset;
            };

            StructuredBuffer<FishBehaviourGPU> fishBuffer;
         #endif

        float4x4 lookAt(float3 at, float3 eye, float3 up) 
		{
            float3 zaxis = normalize(at - eye);
            float3 xaxis = normalize(cross(up, zaxis));
            float3 yaxis = cross(zaxis, xaxis);
            return float4x4
			(
                xaxis.x, yaxis.x, zaxis.x, 0,
                xaxis.y, yaxis.y, zaxis.y, 0,
                xaxis.z, yaxis.z, zaxis.z, 0,
                0, 0, 0, 1
            );
        }
     
         void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				float3 vertexPos = v.vertex.xyz;
				v.vertex.xyz += ((sin(((_Time.w * _AnimationSpeed) + (vertexPos.z * _Yaw) + (vertexPos.y * _Roll))) * _Scale) * float3(1, 0, 0));

                v.vertex = mul(lookAtMatrix, v.vertex);
                v.vertex.xyz += fishPosition;
            #endif
        }

        void setup()
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				FishBehaviourGPU fish = fishBuffer[unity_InstanceID];

				fishPosition = fish.position;
				lookAtMatrix = lookAt(fishPosition, fishPosition + (fish.velocity * -1), float3(0.0, 1.0, 0.0));
            #endif
        }
 
         void surf (Input IN, inout SurfaceOutputStandard o) 
		 {
			 fixed4 c = tex2D(_MainTex, IN.uvcoord) * _Color;
			 o.Albedo = c.rgb;
			 o.Metallic = _Metallic;
			 o.Smoothness = _Glossiness;
			 o.Alpha = c.a;
         }
 
         ENDCG
   }
}