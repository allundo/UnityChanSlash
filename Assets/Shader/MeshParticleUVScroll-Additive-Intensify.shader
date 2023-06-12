Shader "Custom/Particles/MeshScroll Additive Intensify"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Glow ("Intensity", Range(0, 127)) = 1

        _XSpeed("X Scroll Speed", Range(-100.0, 100.0)) = 0.0
        _YSpeed("Y Scroll Speed", Range(-100.0, 100.0)) = 0.0

        _TopPos("Gradation Top Position", Range(0.0001, 0.9999)) = 0.5
        _Focus("Gradation Focus", Range(0, 0.9999)) = 0.0
        _Width("Gradation Top Width", Range(0, 0.9999)) = 0.9999
        _Angle("Gradation Angle", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags { "Queue" = "AlphaTest-50" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha One

        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #include "./CGIncludes/GradationFunctions.cginc"

                sampler2D _MainTex;
                half4 _MainTex_ST;

                half _Glow;

                float _XSpeed;
                float _YSpeed;

                float _TopPos;
                float _Focus;
                float _Width;
                float _Angle;

                float _TopPosY;

                struct vertIn
                {
                    float4 pos      : POSITION;
                    float2 uv       : TEXCOORD0;
                    fixed4 color    : COLOR;
                };

                struct v2f
                {
                    float2 uv       : TEXCOORD0;
                    float2 raw_uv   : TEXCOORD1;
                    float4 pos      : SV_POSITION;
                    fixed4 color    : COLOR;
                };

                v2f vert (vertIn v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.pos);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.raw_uv = v.uv;
                    o.color = v.color;
                    return o;
                }

                fixed4 frag (v2f f) : SV_Target 
                {
                    f.uv.x += _XSpeed * _Time.x;
                    f.uv.y += _YSpeed * _Time.x;

                    fixed4 col = tex2D(_MainTex, f.uv);

                    col *= f.color;
                    col *= _Glow;
                    col.a *= SigmoidGradation(f.raw_uv, _TopPos, _Focus, _Width, _Angle);

                    return col;
                }
            ENDCG
        }
    }
	FallBack "Custom/Mobile/Particles/Additive"
}