Shader "Custom/DoorTransparent" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }

    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 150

        // Pass 1
        Pass {
            Name "Z_BUFFER_ONLY"
            ZWrite On
            ColorMask 0
        }

        // Pass 2
        Name "ALPHA_RENDER"
        Tags { "LightMode"="ForwardBase" }

        CGPROGRAM
            #pragma surface surf Standard fullforwardshadows alpha:fade
            #pragma target 3.0

            sampler2D _MainTex;

            struct Input {
                float2 uv_MainTex;
            };

            fixed4 _Color;

            void surf (Input IN, inout SurfaceOutputStandard o) {
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                o.Alpha = c.a;
            }
        ENDCG
    }

    Fallback "Diffuse"

}
