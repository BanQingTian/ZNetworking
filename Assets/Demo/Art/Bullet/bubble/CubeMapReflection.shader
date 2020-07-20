Shader "Reflection222/CubeMapReflection"
{
	Properties
	{
		_CubeTex("Cube Tex", Cube) = ""{}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 reflectionDir : TEXCOORD0;
			};

			uniform samplerCUBE _CubeTex;

			v2f vert(appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				float3 worldViewDir = WorldSpaceViewDir(v.vertex);
				o.reflectionDir = reflect(-worldViewDir, worldNormal);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = texCUBE(_CubeTex, i.reflectionDir);
				return col;
			}
			ENDCG
		}
	}
}
————————————————
版权声明：本文为CSDN博主「puppet_master」的原创文章，遵循 CC 4.0 BY - SA 版权协议，转载请附上原文出处链接及本声明。
原文链接：https ://blog.csdn.net/puppet_master/article/details/80808486