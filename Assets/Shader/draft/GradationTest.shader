

Shader "Custom/Gradation/GradationTest"
{
    Properties
    {
        _TopColor("Top Color", Color) = (1,1,1,1)
        _BottomColor("Bottom Color", Color) = (1,1,1,1)
        _TopColorPosX("Top Color Pos X", Range(0, 1)) = 0.5
        _TopColorPosY("Top Color Pos Y", Range(0, 1)) =  0.5
        _GradationStrengthX("Gradation Strength X", Range(0, 1)) = 1
        _GradationStrengthY("Gradation Strength Y", Range(0, 1)) = 1
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
            #include "UtilityFunctions.cginc"

            fixed quad_curve(fixed x)
            {
                return x + x * (1.0 - x) * 2.0;
            }

            fixed QuadGradation(fixed x, fixed pos)
            {
                return lerp(quad_curve(x / pos), quad_curve((1 - x) / (1 - pos)), step(pos, x));
            }

            fixed RoundGradation(fixed x, fixed pos)
            {
                return lerp(cos((pos - x) / pos * UNITY_HALF_PI), cos((x - pos) / (1.0 - pos) * UNITY_HALF_PI), step(pos, x));
            }

            fixed QuadGradation(fixed x, fixed gradientTopPos, fixed gradientStrength)
            {
                return 1 - (1 - QuadGradation(x, gradientTopPos)) * gradientStrength;
            }

            fixed RoundGradation(fixed x, fixed gradientTopPos, fixed gradientStrength)
            {
                return 1 - (1 - RoundGradation(x, gradientTopPos)) * gradientStrength;
            }

            float Gradation(fixed2 uv, fixed2 gradientTopPos, fixed2 gradientStrength)
            {
                fixed gradX = RoundGradation(uv.x, gradientTopPos.x, gradientStrength.x);
                fixed gradY = RoundGradation(uv.y, gradientTopPos.y, gradientStrength.y);
                return gradX * gradY;
            }

            fixed4 _TopColor;
            fixed4 _BottomColor;
            fixed  _TopColorPosX;
            fixed  _TopColorPosY;
            fixed  _GradationStrengthX;
            fixed  _GradationStrengthY;

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
                return lerp(_TopColor, _BottomColor, Gradation(i.uv, fixed2(_TopColorPosX, _TopColorPosY), fixed2(_GradationStrengthX, _GradationStrengthY)));
            }

            ENDCG
        }
    }
    FallBack "VertexLit"
}