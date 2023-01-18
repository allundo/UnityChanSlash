Shader "Custom/Mobile/DitherTransparentZWrite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color ("Color", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"}
        LOD 100

        Pass
        {
            ZWrite ON
            ColorMask 0
            Lighting OFF

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "./CGIncludes/DitherTransparentFunctions.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 screenPos : TEXCOORD1;
            };

            half4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
	            o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                DitherClipping(i.screenPos, _Color.a);
                return fixed4(0,0,0,0);
            }
            ENDCG
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
            o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb * _Color.rgb;
        }

        ENDCG
    }
    Fallback "Diffuse"
}
