

Shader "Custom/Gradation/GradationTest2"
{
    Properties
    {
        _FuncType("Function Type", Int) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
            "Queue" = "Transparent"
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            int _FuncType;

            fixed linearf(fixed x)
            {
                return lerp(0.0, 1.0, x);
            }

            fixed bias(fixed b, fixed x)
            {
                return pow(x, log(b) / log(0.5));
            }

            fixed curve(fixed c, fixed x)
            {
                return x + x * (1.0 - x) * c;
            }

            float f_i_curve(float focus, float intensity, float x)
            {
                float height = 1.0 - intensity;
                float c = (height - focus) / (1.0 - focus) / height;
                return curve(c, x);
            }

            float f_i_linear(float focus, float intensity, float x)
            {
                return intensity / (1.0 - focus) * (x - focus) - intensity + 1.0;
            }

            float f_i_curve_linear(float focus, float intensity, float x)
            {
                return lerp(f_i_curve(focus, intensity, x), f_i_linear(focus, intensity, x), step(intensity, x));
            }

            float CurveLinearGradation(float x, float pos, float focus, float intensity)
            {
                return saturate(lerp(f_i_curve_linear(focus, intensity, x / pos), f_i_curve_linear(focus, intensity, (1.0 - x) / (1.0 - pos)), step(pos, x)));
            }

            fixed gain(fixed g, fixed x)
            {
                // 単体で使用する場合はエラーは起きない
                return lerp(bias(1.0 - g, 2.0 * x) * 0.5, 1.0 - bias(1.0 - g, 2.0 - 2.0 * x) * 0.5, step(0.5, x));
            }

            fixed curve_curve(fixed x)
            {
                return lerp(curve(2.0, 2.0 * x) * 0.5, 1.0 - curve(2.0, 2.0 * (1.0 - x)) * 0.5, step(0.5, x));
            }

            fixed bias_bias(fixed g, fixed x)
            {
                // 単体で使用する場合はエラーは起きない
                return lerp(bias(1.0 - g, 2.0 * x) * 0.5, bias(g, 2.0 * (x - 0.5)) * 0.5 + 0.5, step(0.5, x));
            }

            fixed bias_bias2(fixed g, fixed x)
            {
                // 実験用: 後半には入力パラメーター x の変位を含まない bias を接続
                return lerp(bias(1.0 - g, 2.0 * x) * 0.5, bias(0.5, x), step(0.5, x));
            }

            fixed linear_linear(fixed x)
            {
                // 実験用: bias_bias と同様, 後半には入力パラメーター x に変位を含む linearf を接続
                return lerp(linearf(2.0 * x) * 0.5, linearf(2.0 * (x - 0.5)) * 0.5 + 0.5, step(0.5, x));
            }

            // !! エラー !! -> 全面真っ黒
            fixed GainGradation(fixed x)
            {
                // UV 座標 0.0 -> 0.7 の範囲は gain で上昇, 0.7 -> 1.0 の範囲は gain で下降
                return lerp(gain(0.8, x / 0.7), gain(0.8, (1 - x) / 0.3), step(0.7, x));
            }

            fixed LinearGradation(fixed x)
            {
                // UV 座標 0.0 -> 0.7 の範囲は線形で上昇, 0.7 -> 1.0 の範囲は線形で下降
                return lerp(linearf(x / 0.7), linearf((1 - x) / 0.3), step(0.7, x));
            }

            fixed BiasGradation(fixed x)
            {
                // UV 座標 0.0 -> 0.7 の範囲は bias で上昇, 0.7 -> 1.0 の範囲は bias で下降
                return lerp(bias(0.8, x / 0.7), bias(0.8, (1 - x) / 0.3), step(0.7, x));
            }

            // !! エラー !! -> bias(1.0 - g, 2.0 * x) * 0.5 の範囲が黒くなる
            fixed BiasBiasGradation(fixed x)
            {
                // UV 座標 0.0 -> 0.7 の範囲は bias_bias で上昇, 0.7 -> 1.0 の範囲は bias_bias で下降
                return lerp(bias_bias(0.8, x / 0.7), bias_bias(0.8, (1 - x) / 0.3), step(0.7, x));
            }

            fixed BiasBias2Gradation(fixed x)
            {
                // UV 座標 0.0 -> 0.7 の範囲は bias_bias2 で上昇, 0.7 -> 1.0 の範囲は bias_bias2 で下降
                return lerp(bias_bias2(0.8, x / 0.7), bias_bias2(0.8, (1 - x) / 0.3), step(0.7, x));
            }

            fixed LinearLinearGradation(fixed x)
            {
                // UV 座標 0.0 -> 0.7 の範囲は linear_linear の線形で上昇, 0.7 -> 1.0 の範囲は linear_linear の線形で下降
                return lerp(linear_linear(x / 0.7), linear_linear((1 - x) / 0.3), step(0.7, x));
            }

            fixed CurveCurveGradation(fixed x)
            {
                // UV 座標 0.0 -> 0.7 の範囲は linear_linear の線形で上昇, 0.7 -> 1.0 の範囲は linear_linear の線形で下降
                return lerp(linear_linear(x / 0.7), linear_linear((1 - x) / 0.3), step(0.7, x));
            }

            float round_activation(float inflection, float x)
            {
                return lerp((1.0 - cos(x / inflection * UNITY_HALF_PI)) * inflection, sin((x - inflection)/(1.0 - inflection) * UNITY_HALF_PI) * (1.0 - inflection) + inflection, step(inflection, x));
            }

            float RoundGradation(float x, float pos, float radius)
            {
                float inflection = 1.0 - radius;
                return lerp(round_activation(inflection, x / pos), round_activation(inflection, (1.0 - x) / (1.0 - pos)), step(pos, x));
            }

            float RoundGradation(float2 uv, float2 gradientTopPos, float radius)
            {
                float gradX = RoundGradation(uv.x, gradientTopPos.x, radius);
                float gradY = RoundGradation(uv.y, gradientTopPos.y, radius);
                return gradX * gradY;
            }

            struct appdata
            {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
            };

            struct v2f
            {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                fixed amount[20];

                amount[0]  = GainGradation(i.uv.x);          // !! エラー !! 0.0 -> 1.0 全面真っ黒
                amount[1]  = LinearGradation(i.uv.x);
                amount[2]  = BiasGradation(i.uv.x);
                amount[3]  = BiasBiasGradation(i.uv.x);      // !! エラー !! 0.0 -> 0.35, 0.85 -> 1.0 の範囲が黒くなる
                amount[4]  = BiasBias2Gradation(i.uv.x);
                amount[5]  = LinearLinearGradation(i.uv.x);
                amount[6]  = CurveCurveGradation(i.uv.x);
                amount[7]  = round_activation(0.7, i.uv.x);
                amount[8]  = RoundGradation(i.uv.x, 0.5, 0.3);
                amount[9]  = RoundGradation(i.uv, float2(0.5, 0.6), 0.3);

                fixed4 red   = fixed4(1, 0, 0, 1);
                fixed4 green = fixed4(0, 1, 0, 1);

                return lerp(red, green, amount[_FuncType]);
            }

            ENDCG
        }
    }
    FallBack "VertexLit"
}