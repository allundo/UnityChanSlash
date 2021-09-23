Shader "Custom/Unlit/FadeDiscrete"
{
    Properties
    {
        _MainTex ("Fade Rule", 2D) = "white" {}
        [MainColor] _Color ("Color", Color) = (0, 0, 0, 0)
    }

    SubShader {
        Tags { "RenderType" = "Opaque" }
        ZTest [unity_GUIZTestMode]

        CGPROGRAM
        #pragma surface surf Lambert
        #include "UnityCG.cginc"

        struct Input {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;

        // To be controlled from scripts: Material.SetColor("_Color", Color c)
        half4 _Color;

        void surf (Input IN, inout SurfaceOutput o) {
            half3 c = tex2D(_MainTex, IN.uv_MainTex).rgb;
            half rule = (c.r + c.g + c.b) * 0.3333f + 0.0001f;

            // clip() : 値が 0 より小さければピクセルを破棄
            // _Color の alpha から rule の色を引いた値が正なら塗りつぶし
            clip(_Color.a - rule);

            o.Albedo = _Color.rgb;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
