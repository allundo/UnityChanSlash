Shader "Custom/Unlit/UnlitTransparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil {
            Ref 2
            Comp Greater
            Pass Replace
        }

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
                    UNITY_FOG_COORDS(1) // Define [float4 fogCoord : TEXCOORD1;] if fog is enabled
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed4 _Color;

                v2f vert (appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                    // Calcurate fog factor from o.vertex.z and set to o.fogCoord.x
                    // 1:base-color -> 0: fog-color
                    UNITY_TRANSFER_FOG(o,o.vertex);
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
