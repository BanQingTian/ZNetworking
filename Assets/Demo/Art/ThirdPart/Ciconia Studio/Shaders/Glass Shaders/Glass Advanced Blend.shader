// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Ciconia Studio/Effects/Glass/Advanced Blend(Triplanar Projection)" {
    Properties {
        [Space(15)][Header(Main Properties)]
        [Space(10)]_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _DesaturateAlbedo ("Desaturate", Range(0, 1)) = 0
        [Space(25)]_SpecColor ("Specular Color", Color) = (1,1,1,1)
        _SpecGlossMap ("Specular map(Gloss A)", 2D) = "white" {}
        [Space(10)]_SpecularIntensity ("Specular Intensity", Range(0, 2)) = 0.2
        _FresnelStrength ("Fresnel Strength", Range(0, 8)) = 0
        _Glossiness ("Glossiness", Range(0, 2)) = 1
        _Ambientlight ("Ambient light", Range(0, 8)) = 0
        [Space(25)]_BumpMap ("Normal map", 2D) = "bump" {}
        _NormalIntensity ("Normal Intensity", Range(0, 2)) = 1
        [Space(25)]_OcclusionMap ("Ambient Occlusion map", 2D) = "white" {}
        _AoIntensity ("Ao Intensity", Range(0, 2)) = 1
        [Space(25)]_EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionMap ("Emission map", 2D) = "white" {}
        _EmissiveIntensity ("Emissive Intensity", Range(0, 2)) = 1

        [Space(15)][Header(Reflection Properties)]
        [Space(10)]_Cubemap ("Cubemap", Cube) = "_Skybox" {}
        [Space(10)]_ReflectionIntensity ("Reflection Intensity", Range(0, 10)) = 2
        _BlurReflection ("Blur Reflection", Range(0, 8)) = 0

        [Space(15)][Header(Blend Properties)]
        [Space(10)][MaterialToggle] _Invertmask ("Invert mask", Float ) = 0
        _TexAssetMAsk ("Blend Mask (Triplanar Projection)", 2D) = "white" {}
        [Space(10)]_MaskAmount ("Mask Amount", Float ) = 0.5
        _Contrast ("Contrast", Float ) = 1

        [Space(15)][Header(Glass Properties)]
        [Space(10)]_ColorGlass ("Color Glass", Color) = (1,1,1,1)
        [Space(10)]_Refraction ("Refraction", Range(0, 2)) = 0.2
        [Space(10)]_Transparency ("Transparency", Range(0, 1)) = 0.5
        [Space(10)][MaterialToggle] _FresnelOnOff ("Fresnel On/Off ", Float ) = 0
        [MaterialToggle] _FresnelInvert ("Fresnel Invert ", Float ) = 0
        _FresnelThickness ("Fresnel Thickness", Range(0, 5)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="Transparent+50"
            "RenderType"="Transparent"
        }
        GrabPass{ }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            uniform sampler2D _GrabTexture;
            uniform float4 _Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _SpecGlossMap; uniform float4 _SpecGlossMap_ST;
            uniform float _SpecularIntensity;
            uniform float _Glossiness;
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float _NormalIntensity;
            uniform float _Ambientlight;
            uniform sampler2D _OcclusionMap; uniform float4 _OcclusionMap_ST;
            uniform float _FresnelStrength;
            uniform float _AoIntensity;
            uniform sampler2D _EmissionMap; uniform float4 _EmissionMap_ST;
            uniform float4 _EmissionColor;
            uniform float _EmissiveIntensity;
            uniform float _DesaturateAlbedo;
            uniform float _Contrast;
            uniform float _MaskAmount;
            uniform float4 _ColorGlass;
            uniform fixed _Invertmask;
            uniform sampler2D _TexAssetMAsk; uniform float4 _TexAssetMAsk_ST;
            uniform float _Refraction;
            uniform float _ReflectionIntensity;
            uniform float _BlurReflection;
            uniform samplerCUBE _Cubemap;
            uniform float _Transparency;
            uniform fixed _FresnelOnOff;
            uniform fixed _FresnelInvert;
            uniform float _FresnelThickness;
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
                float4 screenPos : TEXCOORD7;
                LIGHTING_COORDS(8,9)
                UNITY_FOG_COORDS(10)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD11;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.screenPos = o.pos;
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.normalDir = normalize(i.normalDir);
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _BumpMap_var = UnpackNormal(tex2D(_BumpMap,TRANSFORM_TEX(i.uv0, _BumpMap)));
                float3 node_7657 = lerp(float3(0,0,1),_BumpMap_var.rgb,_NormalIntensity);
                float3 Normalmap = node_7657;
                float3 normalLocal = Normalmap;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 NormalDirectionMask = saturate(((abs(i.normalDir)*2.0)*0.5));
                float3 node_3867 = NormalDirectionMask;
                float3 node_1717 = (i.posWorld.rgb/2.0);
                float2 GB = node_1717.gb;
                float2 node_6022 = GB;
                float4 _node_1257 = tex2D(_TexAssetMAsk,TRANSFORM_TEX(node_6022, _TexAssetMAsk)); // X Axis FrontBack
                float2 RB = node_1717.rb;
                float2 node_5945 = RB;
                float4 _node_4330 = tex2D(_TexAssetMAsk,TRANSFORM_TEX(node_5945, _TexAssetMAsk)); // Y Axis TopBottom
                float2 RG = node_1717.rg;
                float2 node_1270 = RG;
                float4 _node_3650 = tex2D(_TexAssetMAsk,TRANSFORM_TEX(node_1270, _TexAssetMAsk)); // Z Axis LeftRight 
                float3 node_4567 = (node_3867.r*(_node_1257.rgb*NormalDirectionMask.r) + node_3867.g*(_node_4330.rgb*NormalDirectionMask.g) + node_3867.b*(_node_3650.rgb*NormalDirectionMask.b));
                float node_3462 = 0.0;
                float node_6202 = (1.0+(-1*_Contrast));
                float3 Mask = saturate(((_MaskAmount*2.0+-1.0)+(node_6202 + ( (lerp( node_4567, (1.0 - node_4567), _Invertmask ) - node_3462) * (_Contrast - node_6202) ) / (1.0 - node_3462))));
                float3 Refractionmap = (float3((node_7657.rg*_Refraction),0.0)*Mask);
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + Refractionmap.rg;
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float4 _SpecGlossMap_var = tex2D(_SpecGlossMap,TRANSFORM_TEX(i.uv0, _SpecGlossMap));
                float Glossiness = (_SpecGlossMap_var.a*_Glossiness);
                float gloss = Glossiness;
                float perceptualRoughness = 1.0 - Glossiness;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0+1.0);
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMin[0] = unity_SpecCube0_BoxMin;
                    d.boxMin[1] = unity_SpecCube1_BoxMin;
                #endif
                #if UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMax[0] = unity_SpecCube0_BoxMax;
                    d.boxMax[1] = unity_SpecCube1_BoxMax;
                    d.probePosition[0] = unity_SpecCube0_ProbePosition;
                    d.probePosition[1] = unity_SpecCube1_ProbePosition;
                #endif
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float4 _Cubemap_var = texCUBElod(_Cubemap,float4(viewReflectDirection,_BlurReflection));
                float3 CubemapSpecular = ((((0.95*pow(1.0-max(0,dot(normalDirection, viewDirection)),1.0))+0.05)*_FresnelStrength)+(_Cubemap_var.rgb*(_Cubemap_var.a*_ReflectionIntensity)));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float3 Specularmap = (_SpecColor.rgb*_SpecGlossMap_var.rgb*_SpecularIntensity);
                float3 specularColor = Specularmap;
                float specularMonochrome;
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 Diffusemap = (lerp(_Color.rgb,_ColorGlass.rgb,Mask)*lerp(_MainTex_var.rgb,dot(_MainTex_var.rgb,float3(0.3,0.59,0.11)),_DesaturateAlbedo));
                float3 diffuseColor = Diffusemap; // Need this for specular when using metallic
                diffuseColor = EnergyConservationBetweenDiffuseAndSpecular(diffuseColor, specularColor, specularMonochrome);
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                half surfaceReduction;
                #ifdef UNITY_COLORSPACE_GAMMA
                    surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;
                #else
                    surfaceReduction = 1.0/(roughness*roughness + 1.0);
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (gi.indirect.specular + CubemapSpecular);
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                indirectSpecular *= surfaceReduction;
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                float Ambientlight = _Ambientlight;
                float node_6535 = Ambientlight;
                indirectDiffuse += float3(node_6535,node_6535,node_6535); // Diffuse Ambient Light
                indirectDiffuse += gi.indirect.diffuse;
                float4 _OcclusionMap_var = tex2D(_OcclusionMap,TRANSFORM_TEX(i.uv0, _OcclusionMap));
                float Aomap = saturate((1.0-(1.0-_OcclusionMap_var.r)*(1.0-lerp(1,0,_AoIntensity))));
                indirectDiffuse *= Aomap; // Diffuse AO
                diffuseColor *= 1-specularMonochrome;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float4 _EmissionMap_var = tex2D(_EmissionMap,TRANSFORM_TEX(i.uv0, _EmissionMap));
                float3 Emissivemap = ((_EmissionColor.rgb*_EmissionMap_var.rgb)*_EmissiveIntensity);
                float3 emissive = Emissivemap;
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                float node_1894 = 0.0;
                float node_4735 = (1.0+(-1*_FresnelThickness));
                float node_8083 = pow(1.0-max(0,dot(normalDirection, viewDirection)),(_Transparency+lerp(0,1,(node_4735 + ( (1.0 - node_1894) * (_FresnelThickness - node_4735) ) / (1.0 - node_1894)))));
                float Transparency = lerp( lerp(1,0,_Transparency), saturate(lerp( node_8083, (1.0 - node_8083), _FresnelInvert )), _FresnelOnOff );
                fixed4 finalRGBA = fixed4(lerp(sceneColor.rgb, finalColor,Transparency),1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
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
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            uniform sampler2D _GrabTexture;
            uniform float4 _Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _SpecGlossMap; uniform float4 _SpecGlossMap_ST;
            uniform float _SpecularIntensity;
            uniform float _Glossiness;
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float _NormalIntensity;
            uniform sampler2D _EmissionMap; uniform float4 _EmissionMap_ST;
            uniform float4 _EmissionColor;
            uniform float _EmissiveIntensity;
            uniform float _DesaturateAlbedo;
            uniform float _Contrast;
            uniform float _MaskAmount;
            uniform float4 _ColorGlass;
            uniform fixed _Invertmask;
            uniform sampler2D _TexAssetMAsk; uniform float4 _TexAssetMAsk_ST;
            uniform float _Refraction;
            uniform float _Transparency;
            uniform fixed _FresnelOnOff;
            uniform fixed _FresnelInvert;
            uniform float _FresnelThickness;
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
                float4 screenPos : TEXCOORD7;
                LIGHTING_COORDS(8,9)
                UNITY_FOG_COORDS(10)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.screenPos = o.pos;
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.normalDir = normalize(i.normalDir);
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _BumpMap_var = UnpackNormal(tex2D(_BumpMap,TRANSFORM_TEX(i.uv0, _BumpMap)));
                float3 node_7657 = lerp(float3(0,0,1),_BumpMap_var.rgb,_NormalIntensity);
                float3 Normalmap = node_7657;
                float3 normalLocal = Normalmap;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 NormalDirectionMask = saturate(((abs(i.normalDir)*2.0)*0.5));
                float3 node_3867 = NormalDirectionMask;
                float3 node_1717 = (i.posWorld.rgb/2.0);
                float2 GB = node_1717.gb;
                float2 node_6022 = GB;
                float4 _node_1257 = tex2D(_TexAssetMAsk,TRANSFORM_TEX(node_6022, _TexAssetMAsk)); // X Axis FrontBack
                float2 RB = node_1717.rb;
                float2 node_5945 = RB;
                float4 _node_4330 = tex2D(_TexAssetMAsk,TRANSFORM_TEX(node_5945, _TexAssetMAsk)); // Y Axis TopBottom
                float2 RG = node_1717.rg;
                float2 node_1270 = RG;
                float4 _node_3650 = tex2D(_TexAssetMAsk,TRANSFORM_TEX(node_1270, _TexAssetMAsk)); // Z Axis LeftRight 
                float3 node_4567 = (node_3867.r*(_node_1257.rgb*NormalDirectionMask.r) + node_3867.g*(_node_4330.rgb*NormalDirectionMask.g) + node_3867.b*(_node_3650.rgb*NormalDirectionMask.b));
                float node_3462 = 0.0;
                float node_6202 = (1.0+(-1*_Contrast));
                float3 Mask = saturate(((_MaskAmount*2.0+-1.0)+(node_6202 + ( (lerp( node_4567, (1.0 - node_4567), _Invertmask ) - node_3462) * (_Contrast - node_6202) ) / (1.0 - node_3462))));
                float3 Refractionmap = (float3((node_7657.rg*_Refraction),0.0)*Mask);
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + Refractionmap.rg;
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float4 _SpecGlossMap_var = tex2D(_SpecGlossMap,TRANSFORM_TEX(i.uv0, _SpecGlossMap));
                float Glossiness = (_SpecGlossMap_var.a*_Glossiness);
                float gloss = Glossiness;
                float perceptualRoughness = 1.0 - Glossiness;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float3 Specularmap = (_SpecColor.rgb*_SpecGlossMap_var.rgb*_SpecularIntensity);
                float3 specularColor = Specularmap;
                float specularMonochrome;
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 Diffusemap = (lerp(_Color.rgb,_ColorGlass.rgb,Mask)*lerp(_MainTex_var.rgb,dot(_MainTex_var.rgb,float3(0.3,0.59,0.11)),_DesaturateAlbedo));
                float3 diffuseColor = Diffusemap; // Need this for specular when using metallic
                diffuseColor = EnergyConservationBetweenDiffuseAndSpecular(diffuseColor, specularColor, specularMonochrome);
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                diffuseColor *= 1-specularMonochrome;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                float node_1894 = 0.0;
                float node_4735 = (1.0+(-1*_FresnelThickness));
                float node_8083 = pow(1.0-max(0,dot(normalDirection, viewDirection)),(_Transparency+lerp(0,1,(node_4735 + ( (1.0 - node_1894) * (_FresnelThickness - node_4735) ) / (1.0 - node_1894)))));
                float Transparency = lerp( lerp(1,0,_Transparency), saturate(lerp( node_8083, (1.0 - node_8083), _FresnelInvert )), _FresnelOnOff );
                fixed4 finalRGBA = fixed4(finalColor * Transparency,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
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
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x xboxone ps4 psp2 n3ds wiiu 
            #pragma target 3.0
            uniform float4 _Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _SpecGlossMap; uniform float4 _SpecGlossMap_ST;
            uniform float _SpecularIntensity;
            uniform float _Glossiness;
            uniform sampler2D _EmissionMap; uniform float4 _EmissionMap_ST;
            uniform float4 _EmissionColor;
            uniform float _EmissiveIntensity;
            uniform float _DesaturateAlbedo;
            uniform float _Contrast;
            uniform float _MaskAmount;
            uniform float4 _ColorGlass;
            uniform fixed _Invertmask;
            uniform sampler2D _TexAssetMAsk; uniform float4 _TexAssetMAsk_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
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
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                float4 _EmissionMap_var = tex2D(_EmissionMap,TRANSFORM_TEX(i.uv0, _EmissionMap));
                float3 Emissivemap = ((_EmissionColor.rgb*_EmissionMap_var.rgb)*_EmissiveIntensity);
                o.Emission = Emissivemap;
                
                float3 NormalDirectionMask = saturate(((abs(i.normalDir)*2.0)*0.5));
                float3 node_3867 = NormalDirectionMask;
                float3 node_1717 = (i.posWorld.rgb/2.0);
                float2 GB = node_1717.gb;
                float2 node_6022 = GB;
                float4 _node_1257 = tex2D(_TexAssetMAsk,TRANSFORM_TEX(node_6022, _TexAssetMAsk)); // X Axis FrontBack
                float2 RB = node_1717.rb;
                float2 node_5945 = RB;
                float4 _node_4330 = tex2D(_TexAssetMAsk,TRANSFORM_TEX(node_5945, _TexAssetMAsk)); // Y Axis TopBottom
                float2 RG = node_1717.rg;
                float2 node_1270 = RG;
                float4 _node_3650 = tex2D(_TexAssetMAsk,TRANSFORM_TEX(node_1270, _TexAssetMAsk)); // Z Axis LeftRight 
                float3 node_4567 = (node_3867.r*(_node_1257.rgb*NormalDirectionMask.r) + node_3867.g*(_node_4330.rgb*NormalDirectionMask.g) + node_3867.b*(_node_3650.rgb*NormalDirectionMask.b));
                float node_3462 = 0.0;
                float node_6202 = (1.0+(-1*_Contrast));
                float3 Mask = saturate(((_MaskAmount*2.0+-1.0)+(node_6202 + ( (lerp( node_4567, (1.0 - node_4567), _Invertmask ) - node_3462) * (_Contrast - node_6202) ) / (1.0 - node_3462))));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 Diffusemap = (lerp(_Color.rgb,_ColorGlass.rgb,Mask)*lerp(_MainTex_var.rgb,dot(_MainTex_var.rgb,float3(0.3,0.59,0.11)),_DesaturateAlbedo));
                float3 diffColor = Diffusemap;
                float4 _SpecGlossMap_var = tex2D(_SpecGlossMap,TRANSFORM_TEX(i.uv0, _SpecGlossMap));
                float3 Specularmap = (_SpecColor.rgb*_SpecGlossMap_var.rgb*_SpecularIntensity);
                float3 specColor = Specularmap;
                float specularMonochrome = max(max(specColor.r, specColor.g),specColor.b);
                diffColor *= (1.0-specularMonochrome);
                float Glossiness = (_SpecGlossMap_var.a*_Glossiness);
                float roughness = 1.0 - Glossiness;
                o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
