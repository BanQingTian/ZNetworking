Shader "NReal/WaterShader" {
    Properties {
        _Diffuse ("Diffuse", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _node_9784 ("node_9784", 2D) = "white" {}
        _waveIntensity ("waveIntensity", Range(0, 1)) = 0.2096793
        _AlphaMap ("AlphaMap", 2D) = "white" {}
        _Alpha ("Alpha", Range(0, 1)) = 0.7480709
        _cubemap ("cubemap", Cube) = "_Skybox" {}
//        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        GrabPass{ }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define _Attenuation 1
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            //#pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _GrabTexture;
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform float4 _Color;
            uniform sampler2D _node_9784; uniform float4 _node_9784_ST;
            uniform float _waveIntensity;
            uniform samplerCUBE _cubemap;
            uniform sampler2D _AlphaMap; uniform float4 _AlphaMap_ST;
            uniform float _Alpha;
//            float _Attenuation;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                float4 projPos : TEXCOORD4;
                float3 viewDirection : TEXCOORD5;
                float3 viewReflectDirection : TEXCOORD6;
                float3 lightDirection : TEXCOORD7;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.viewDirection = normalize(_WorldSpaceCameraPos.xyz - o.posWorld.xyz);  
                o.pos = UnityObjectToClipPos( v.vertex );
                o.viewReflectDirection = reflect( -o.viewDirection, o.normalDir );
                o.lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                o.projPos = ComputeScreenPos (o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float2 node_8864 = float2((0.0*_Time.g),(_Time.g*(-0.1)));
                float2 node_1184 = (i.uv0+node_8864);
                float4 _node_9784_var = tex2D(_node_9784,TRANSFORM_TEX(node_1184, _node_9784));
                float2 node_6562 = (float2(_node_9784_var.r,_node_9784_var.r)*_waveIntensity);
                float4 _AlphaMap_var = tex2D(_AlphaMap,TRANSFORM_TEX(i.uv1, _AlphaMap));
                float2 sceneUVs = (i.projPos.xy / i.projPos.w) + (node_6562*_AlphaMap_var.r);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
//                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float3 attenColor = _Attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( i.normalDir, i.lightDirection ));   
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                indirectDiffuse += (texCUBE(_cubemap,i.viewReflectDirection).rgb*(1.0-max(0,dot(i.normalDir, i.viewDirection)))); // Diffuse Ambient Light              
                float2 node_6924 = (i.uv0+node_6562+(0.3*node_8864));
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(node_6924, _Diffuse));
                float3 diffuseColor = (_Diffuse_var.rgb*_Color.rgb);
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                return fixed4(lerp(sceneColor.rgb, finalColor,(_AlphaMap_var.r*_Alpha)),1);

            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd
//            #pragma multi_compile_fog
//            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _GrabTexture;
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform float4 _Color;
            uniform sampler2D _node_9784; uniform float4 _node_9784_ST;
            uniform float _waveIntensity;
            uniform sampler2D _AlphaMap; uniform float4 _AlphaMap_ST;
            uniform float _Alpha;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 posWorld : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                float4 projPos : TEXCOORD4;
                float3 lightDirection : TEXCOORD8;
                LIGHTING_COORDS(5,6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
//                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - o.posWorld.xyz,_WorldSpaceLightPos0.w));      
                o.projPos = ComputeScreenPos (o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float2 node_8864 = float2((0.0*_Time.g),(_Time.g*(-0.1)));
                float2 node_1184 = (i.uv0+node_8864);
                float4 _node_9784_var = tex2D(_node_9784,TRANSFORM_TEX(node_1184, _node_9784));
                float2 node_6562 = (float2(_node_9784_var.r,_node_9784_var.r)*_waveIntensity);
                float4 _AlphaMap_var = tex2D(_AlphaMap,TRANSFORM_TEX(i.uv1, _AlphaMap));
                float2 sceneUVs = (i.projPos.xy / i.projPos.w) + (node_6562*_AlphaMap_var.r);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
//                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( i.normalDir, i.lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float2 node_6924 = (i.uv0+node_6562+(0.3*node_8864));
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(node_6924, _Diffuse));
                float3 diffuseColor = (_Diffuse_var.rgb*_Color.rgb);
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                return fixed4(diffuse * (_AlphaMap_var.r*_Alpha),0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
