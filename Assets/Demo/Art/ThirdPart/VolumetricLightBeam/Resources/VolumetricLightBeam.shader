Shader "VolumetricLightBeam/Beam"
{
Properties
{
    _ConeSlopeCosSin("Cone Slope Cos Sin", Vector) = (0,0,0,0)
    _ConeRadius("Cone Radius", Vector) = (0,0,0,0)
    _ConeApexOffsetZ("Cone Apex Offset Z", Float) = 0

    [HDR] _Color("Color", Color) = (1,1,1,1)
    _AlphaInside("Alpha Inside", Range(0,1)) = 1
    _AlphaOutside("Alpha Outside", Range(0,1)) = 1

    _DistanceFadeStart("Distance Fade Start", Float) = 0
    _DistanceFadeEnd("Distance Fade End", Float) = 1

    _DistanceCamClipping("Camera Clipping Distance", Float) = 0.5

    _AttenuationLerpLinearQuad("Lerp between attenuation linear and quad", Float) = 0.5
    _DepthBlendDistance("Depth Blend Distance", Float) = 2

    _FresnelPow("Fresnel Pow", Range(0,15)) = 1

    _GlareFrontal("Glare Frontal", Range(0,1)) = 0.5
    _GlareBehind("Glare from Behind", Range(0,1)) = 0.5

    _NoiseLocal("Noise Local", Vector) = (0,0,0,0)
    _NoiseParam("Noise Param", Vector) = (0,0,0,0)

    _CameraParams("Camera Params", Vector) = (0,0,0,0)
    _ClippingPlaneWS("Clipping Plane WS", Vector) = (0,0,0,0)
}

Category
{
    Tags
    {
        "Queue"="Transparent"
        "RenderType"="Transparent"
        "IgnoreProjector" = "True"
    } 

    Blend One One
    ZWrite Off

    SubShader
    {
	    // INSIDE
	    Pass
        {
            Cull Front

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile __ VLB_NOISE_3D
            #pragma multi_compile __ VLB_DEPTH_BLEND
            #pragma multi_compile __ VLB_CLIPPING_PLANE

            #include "VolumetricLightBeamShared.cginc"

            half4 frag (v2f i) : SV_Target
            {
                return fragShared(i, 0);
            }

            ENDCG
        }
        
        // OUTSIDE
        Pass
        {
            Cull Back

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile __ VLB_NOISE_3D
            #pragma multi_compile __ VLB_DEPTH_BLEND
            #pragma multi_compile __ VLB_CLIPPING_PLANE

            #include "VolumetricLightBeamShared.cginc"

            half4 frag(v2f i) : SV_Target
            {
                return fragShared(i, 1);
            }

            ENDCG
        }
    }
}
}
