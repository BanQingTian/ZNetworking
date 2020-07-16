Shader "NReal/Waterfall" {
    Properties {
        _Diffuse ("Diffuse", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _wave ("wave", 2D) = "white" {}
        _waveIntensity ("waveIntensity", Range(0, 1)) = 0.606631
        _AlphaMap ("AlphaMap", 2D) = "white" {}
        _Alpha ("Alpha", Range(0, 1)) = 0.3386314
        _cubemap ("cubemap", Cube) = "_Skybox" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
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
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
//            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase
            #pragma target 3.0
            uniform sampler2D _GrabTexture;
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform float4 _Color;
            uniform sampler2D _wave; uniform float4 _wave_ST;
            uniform float _waveIntensity;
            uniform sampler2D _AlphaMap; uniform float4 _AlphaMap_ST;
            uniform float _Alpha;
            samplerCUBE _cubemap;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
//                float2 uv2 : TEXCOORD2;
				float3 lightDirection : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                float4 projPos : TEXCOORD7;
                float3 viewDirection : TEXCOORD8;
                float3 viewReflectDirection : TEXCOORD9;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.viewDirection = normalize(_WorldSpaceCameraPos.xyz - o.posWorld.xyz);  
//              float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.viewReflectDirection = reflect( -o.viewDirection, o.normalDir );
                o.lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                o.projPos = ComputeScreenPos (o.pos);
                //COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float2 node_8780 = float2(((-0.5)*_Time.g),(_Time.g*0.0));
                float2 node_1464 = (i.uv0+node_8780);
                float4 _wave_var = tex2D(_wave,TRANSFORM_TEX(node_1464, _wave));
                float2 node_168 = (float2(_wave_var.r,_wave_var.r)*_waveIntensity);
                float4 _AlphaMap_var = tex2D(_AlphaMap,TRANSFORM_TEX(i.uv1, _AlphaMap));
                float2 sceneUVs = (i.projPos.xy / i.projPos.w) + (node_168*_AlphaMap_var.r);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
//              float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( i.normalDir, i.lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                indirectDiffuse += (texCUBE(_cubemap,i.viewReflectDirection).rgb*(1.0-max(0,dot(i.normalDir, i.viewDirection)))); // Diffuse Ambient Light  
                float2 node_9906 = (i.uv0+node_168+(1.0*node_8780));
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(node_9906, _Diffuse));
                float3 diffuseColor = (_Diffuse_var.rgb*_Color.rgb);
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                return fixed4(lerp(sceneColor.rgb, diffuse,(_AlphaMap_var.r*_Alpha)),1);
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
//          #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd
            #pragma target 3.0
            uniform sampler2D _GrabTexture;
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform float4 _Color;
            uniform sampler2D _wave; uniform float4 _wave_ST;
            uniform float _waveIntensity;
            uniform sampler2D _AlphaMap; uniform float4 _AlphaMap_ST;
            uniform float _Alpha;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                float4 projPos : TEXCOORD7;
                float3 normalDirection : TEXCOORD10;
                float3 lightColor : TEXCOORD11;
                LIGHTING_COORDS(8,9)
            };
            VertexOutput vert (VertexInput v) {
				VertexOutput o;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
//              o.lightColor = _LightColor0.rgb;
                o.normalDirection = o.normalDir;
                
                o.pos = UnityObjectToClipPos( v.vertex );
                o.projPos = ComputeScreenPos (o.pos);
                //COMPUTE_EYEDEPTH(o.projPos.z);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float2 node_8780 = float2(((-0.5)*_Time.g),(_Time.g*0.0));
                float2 node_1464 = (i.uv0+node_8780);
                float4 _wave_var = tex2D(_wave,TRANSFORM_TEX(node_1464, _wave));
                float2 node_168 = (float2(_wave_var.r,_wave_var.r)*_waveIntensity);
                float4 _AlphaMap_var = tex2D(_AlphaMap,TRANSFORM_TEX(i.uv1, _AlphaMap));
                float2 sceneUVs = (i.projPos.xy / i.projPos.w) + (node_168*_AlphaMap_var.r);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
//              i.lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( i.normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float2 node_9906 = (i.uv0+node_168+(1.0*node_8780));
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(node_9906, _Diffuse));
                float3 diffuseColor = (_Diffuse_var.rgb*_Color.rgb);
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                return fixed4(diffuse * (_AlphaMap_var.r*_Alpha),0);
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
//          #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
//          #pragma only_renderers d3d9 d3d11 glcore gles n3ds wiiu 
            #pragma target 3.0
            uniform sampler2D _Diffuse; uniform float4 _Diffuse_ST;
            uniform float4 _Color;
            uniform sampler2D _wave; uniform float4 _wave_ST;
            uniform float _waveIntensity;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 viewDirection : TEXCOORD4;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.viewDirection = normalize(_WorldSpaceCameraPos.xyz - o.posWorld.xyz);  
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );    
                o.Emission = 0;
                float2 node_8780 = float2(((-0.5)*_Time.g),(_Time.g*0.0));
                float2 node_1464 = (i.uv0+node_8780);
                float4 _wave_var = tex2D(_wave,TRANSFORM_TEX(node_1464, _wave));
                float2 node_168 = (float2(_wave_var.r,_wave_var.r)*_waveIntensity);
                float2 node_9906 = (i.uv0+node_168+(1.0*node_8780));
                float4 _Diffuse_var = tex2D(_Diffuse,TRANSFORM_TEX(node_9906, _Diffuse));
                float3 diffColor = (_Diffuse_var.rgb*_Color.rgb);
                o.Albedo = diffColor;    
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
