Shader "Custom/Unlit/UVScroll"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _XSpeed("X Scroll Speed", Range(0.0, 100.0)) = 0.0
        _YSpeed("Y Scroll Speed", Range(0.0, 100.0)) = 0.0
    }
    SubShader
    {
        Tags
        {
            "Queue"      = "Transparent"
            "RenderType" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _XSpeed;
            float _YSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                i.uv.x = i.uv.x + _XSpeed * _Time.x;
                i.uv.y = i.uv.y + _YSpeed * _Time.x;

                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
    FallBack "VertexLit"
}