Shader "UnityChan/Skin"
{
	Properties
	{
		_Color ("Base Color", Color) = (1, 1, 1, 1)
		[MainColor] _AdditiveColor ("Additive Color", Color) = (0, 0, 0, 0)
		_ShadowColor ("Shadow Color", Color) = (0.8, 0.8, 1, 1)
		_EdgeThickness ("Outline Thickness", Float) = 1

		_MainTex ("Diffuse", 2D) = "white" {}
		_FalloffSampler ("Falloff Control", 2D) = "white" {}
		_RimLightSampler ("RimLight Control", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
			"Queue"="Geometry"
			"LightMode"="ForwardBase"
		}

		Pass
		{
			Cull Back
			ZTest LEqual
CGPROGRAM
#pragma multi_compile_fwdbase
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "CharaSkinAdditive.cginc"
ENDCG
		}

		Pass
		{
			Cull Front
			ZTest Less
CGPROGRAM
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
#include "CharaOutlineAdditive.cginc"
ENDCG
		}

	}

	FallBack "Transparent/Cutout/Diffuse"
}
