Shader "Custom/Unlit/UnlitRotate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color("Color", Color) = (1,1,1,1)
        _Rotate ("TextureRotation", Vector) = (1,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
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
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed4 _Color;
                fixed4 _Rotate;

                v2f vert (appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(mul(v.uv, half2x2(_Rotate.x, _Rotate.y, _Rotate.z, _Rotate.w)), _MainTex);
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    return tex2D(_MainTex, i.uv) * _Color;
                }
            ENDCG
        }
    }
}
