Shader "Custom/Unlit/RoundGradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)

        _TopPosX("Gradation Top Position X", Range(0, 1)) = 0.5
        _TopPosY("Gradation Top Position Y", Range(0, 1)) = 0.5

        _Radius("Gradation Top Radius", Range(0, 1)) = 0.0
        _Focus("Gradation Focus", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags
        {
            "Queue"      = "Transparent"
            "RenderType" = "Transparent"
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
            #include "./CGIncludes/GradationFunctions.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 raw_uv : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Color;

            float _TopPosX;
            float _TopPosY;

            float _Radius;
            float _Focus;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.raw_uv = v.uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv) * _Color;
                col.a *= RoundGradation(i.raw_uv, float2(_TopPosX, _TopPosY), _Focus, _Radius);

                return col;
            }
            ENDCG
        }
    }
    FallBack "VertexLit"
}
