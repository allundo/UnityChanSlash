Shader "Custom/Mobile/DitherTransparentZWrite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color ("Color", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags {"Queue"="Transparent+1" "RenderType"="TransparentCutout"}
        LOD 100

        Pass{
  	        ZWrite ON
  	        ColorMask 0
        }

        CGPROGRAM
        #pragma surface surf Lambert
        #include "UnityCG.cginc"
        #include "./CGIncludes/DitherTransparentFunctions.cginc"

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };

        sampler2D _MainTex;

        // To be controlled from scripts: Material.SetColor("_Color", Color c)
        half4 _Color;

        void surf (Input IN, inout SurfaceOutput o)
        {
            DitherClipping(IN.screenPos, _Color.a);
            o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
        }

        ENDCG
    }
    Fallback "Diffuse"
}
