// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Custom/Mobile/Diffuse-Additive-Trail" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [MainColor] _AdditiveColor ("Additive Color", Color) = (0, 0, 0, 1)
        _NoiseTex ("Noise", 2D) = "white" {}
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 150

        CGPROGRAM
        #pragma surface surf Lambert noforwardadd vertex:vert

        sampler2D _MainTex;
        sampler2D _NoiseTex;
        sampler2D _DitherMaskLOD2D;

        float4 _AdditiveColor;
        fixed4 _TrailDir;

        struct Input {
            float2 uv_MainTex;
            float4 screenPos;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            float weight = clamp(dot(v.normal, _TrailDir), 0, 1);
            float noise = 1 + tex2Dlod(_NoiseTex, v.texcoord).r * 0.5;
            fixed4 trail = _TrailDir * weight * noise;
            v.vertex.xyz += trail.xyz;
        }

        void surf (Input IN, inout SurfaceOutput o) {
            float2 vpos = IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy * 0.25;

            // ディザパターンを 1 px ずらす(半透明同士が重なった時の点滅防止)
            vpos.x += 0.25;

            vpos.y = _AdditiveColor.a * 0.9375 + frac(vpos.y) * 0.0625;

            clip(tex2D(_DitherMaskLOD2D, vpos).a - 0.5);

            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb + _AdditiveColor.rgb;
        }

        ENDCG
    }

    Fallback "Mobile/VertexLit"
}