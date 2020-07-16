//Thanks Chaser324 for sharing

Shader "nreal/N_Wireframe"
{
	Properties
	{
		_WireThickness ("Wire Thickness", RANGE(0, 800)) = 100
		_WireSmoothness ("Wire Smoothness", RANGE(0, 20)) = 3
		_WireColor ("Wire Color", Color) = (0.0, 1.0, 0.0, 1.0)
		_BaseColor ("Base Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_MaxTriSize("Max Tri Size", RANGE(0, 200)) = 25
		_Mask ("Mask", 2D) = "white"
		_MaskAll("MaskAll", 2D) = "white"

	}

	SubShader
	{
		Tags { 
			"RenderType"="Opaque"
		}
		cull back
		blend srccolor One

		Pass
		{
			// Wireframe shader based on the the following
			// http://developer.download.nvidia.com/SDK/10/direct3d/Source/SolidWireframe/Doc/SolidWireframe.pdf

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Wireframe.cginc"

			ENDCG
		}
	}
}
