Shader "Custom/Unlit/UVScrollWithGradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)

        _XSpeed("X Scroll Speed", Range(-100.0, 100.0)) = 0.0
        _YSpeed("Y Scroll Speed", Range(-100.0, 100.0)) = 0.0

        _TopPos("Gradation Top Position", Range(0.0001, 0.9999)) = 0.5
        _Focus("Gradation Focus", Range(0, 0.9999)) = 0.0
        _Width("Gradation Top Width", Range(0, 0.9999)) = 0.9999
        _Angle("Gradation Angle", Range(0, 1)) = 0.0
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

            float _XSpeed;
            float _YSpeed;

            float _TopPos;
            float _Focus;
            float _Width;
            float _Angle;

            float _TopPosY;

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
                i.uv.x += _XSpeed * _Time.x;
                i.uv.y += _YSpeed * _Time.x;

                float4 col = tex2D(_MainTex, i.uv) * _Color;
                col.a *= LinearGradation(i.raw_uv, _TopPos, _Focus, _Width, _Angle);

                return col;
            }
            ENDCG
        }
    }
    FallBack "VertexLit"
}
