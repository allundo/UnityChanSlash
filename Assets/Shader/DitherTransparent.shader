Shader "Custom/DitherTransparent" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color ("Color", Color) = (0, 0, 0, 0)
    }

    SubShader {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert
        #include "UnityCG.cginc"

        struct Input {
            float2 uv_MainTex;
            float4 screenPos;
        };

        sampler2D _MainTex;
        sampler2D _DitherMaskLOD2D;

        // To be controlled from scripts: Material.SetColor("_Color", Color c)
        half4 _Color;

        void surf (Input IN, inout SurfaceOutput o) {
            // スクリーン座標(VPOS) から ディザテクスチャ(DitherMaskLOD2D) へのマッピング
            // 4 x 4px 毎にテクスチャをマッピングする
            float2 vpos = IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy * 0.25;

            // DitherMaskLOD2D は Y 座標方向 (4*16px) に透明度が設定されている
            // _Color.a(0 〜 1) の値から Y のオフセット(0/16 〜 15/16 = 0.935)を決定
            // (スクリーン座標 / 4) の小数部分が 4 x 4 タイル内のオフセット(1/16 = 0.0625)に対応
            vpos.y = _Color.a * 0.9375 + frac(vpos.y) * 0.0625;

            // clip() : 値が 0 より小さければピクセルを破棄
            // ディザの値はAチャンネルに0,1で格納されている
            clip(tex2D(_DitherMaskLOD2D, vpos).a - 0.5);

            o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
