Shader "Custom/Unlit/UnlitTransparentRotate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color("Color", Color) = (1,1,1,1)
        _Angle ("Angle", Range(-1.0, 1.0)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Angle;

            v2f vert(appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float angle = _Angle * UNITY_TWO_PI;

                // Pivot
                float2 pivot = float2(0.5, 0.5);
                // Rotation Matrix
                float cosAngle = cos(angle);
                float sinAngle = sin(angle);
                float2x2 rot = float2x2(cosAngle, -sinAngle, sinAngle, cosAngle);

                // Rotation consedering pivot
                float2 uv = v.texcoord.xy - pivot;
                o.uv = mul(rot, uv);
                o.uv += pivot;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                return col;
            }
            ENDCG
        }
    }
}
