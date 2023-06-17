Shader "Custom/Simple/UI/Particles/Additive Intensify"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _Glow ("Intensity", Range(0, 5)) = 1
    }

    Category {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha One
        ColorMask RGB
        Cull Off Lighting Off ZWrite Off
        ZTest [unity_GUIZTestMode]

        SubShader
        {
            Pass
            {
                CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #include "UnityCG.cginc"
                    #pragma target 2.0
                    #pragma multi_compile_particles

                    sampler2D _MainTex;
                    half4 _MainTex_ST;
                    half _Glow;

                    struct vertIn
                    {
                        float4 pos : POSITION;
                        half2 tex : TEXCOORD0;
                        fixed4 color : COLOR;
                    };

                    struct v2f
                    {
                        float4 pos : SV_POSITION;
                        half2 tex : TEXCOORD0;
                        fixed4 color : COLOR;
                    };

                    v2f vert (vertIn v)
                    {
                        v2f o;
                        o.pos = UnityObjectToClipPos(v.pos);
                        o.tex = v.tex * _MainTex_ST.xy + _MainTex_ST.zw;
                        o.color = v.color;
                        return o;
                    }

                    fixed4 frag (v2f f) : SV_Target 
                    {
                        fixed4 col = tex2D(_MainTex, f.tex);
                        col *= f.color;
                        col *= _Glow;
                        return col;
                    }
                ENDCG
            }
        }
    }
    FallBack "Custom/Mobile/Particles/Additive"
}
