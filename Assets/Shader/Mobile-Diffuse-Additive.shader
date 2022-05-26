// ## CUSTOMIZED
// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Custom/Mobile/Diffuse-Additive"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [MainColor] _AdditiveColor ("Additive Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150

        CGPROGRAM
        #pragma surface surf Lambert noforwardadd
        #include "./CGIncludes/DitherTransparentFunctions.cginc"

        sampler2D _MainTex;

        float4 _AdditiveColor;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            DitherClipping(IN.screenPos, _AdditiveColor.a, 1);
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb + _AdditiveColor.rgb;
        }

        ENDCG
    }

    Fallback "Mobile/VertexLit"
}
