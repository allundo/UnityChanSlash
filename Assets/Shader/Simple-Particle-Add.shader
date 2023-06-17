Shader "Custom/Simple/Particles/Additive"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
    }

    Category
    {
        Tags { "Queue"="AlphaTest-50" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha One
        ColorMask RGB
        Cull Off Lighting Off ZWrite Off

        SubShader
        {
            Pass
            {
                CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma target 2.0
                    #pragma multi_compile_particles

                    #include "UnityCG.cginc"

                    sampler2D _MainTex;
                    half4 _MainTex_ST;

                    struct appdata_t
                    {
                        float4 vertex : POSITION;
                        float2 tex : TEXCOORD0;
                        fixed4 color : COLOR;
                    };

                    struct v2f
                    {
                        float4 vertex : SV_POSITION;
                        float2 tex : TEXCOORD0;
                        fixed4 color : COLOR;
                    };

                    v2f vert (appdata_t v)
                    {
                        v2f o;
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        o.tex = v.tex * _MainTex_ST.xy + _MainTex_ST.zw;
                        o.color = v.color;
                        return o;
                    }

                    fixed4 frag (v2f i) : SV_Target
                    {
                        return i.color * tex2D(_MainTex, i.tex);
                    }
                ENDCG
            }
        }
    }
}
