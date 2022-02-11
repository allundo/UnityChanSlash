Shader "Custom/Mobile/UnlitBaseAndAdditive" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        [MainColor] _AdditiveColor ("Additive Color", Color) = (0, 0, 0, 1)
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 150

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "./CGIncludes/DitherTransparentFunctions.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _AdditiveColor;
            float4 _BaseColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv) * _BaseColor;
                DitherClipping(i.screenPos, col.a * _AdditiveColor.a, 1);
                col.rgb += _AdditiveColor.rgb;
                return col;
            }

            ENDCG
        }
    }
    Fallback "Mobile/VertexLit"
}
