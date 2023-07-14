Shader "Custom/Simple/Particles/Multiply"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
    }

    CGINCLUDE
        #define FOG_EXP2
    ENDCG

    Category
    {
        Tags { "Queue"="AlphaTest-50" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend Zero SrcColor
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

                    struct appdata
                    {
                        float4 vertex : POSITION;
                        fixed4 color : COLOR;
                        float2 uv : TEXCOORD0;
                    };

                    struct v2f
                    {
                        float4 vertex : SV_POSITION;
                        fixed4 color : COLOR;
                        float2 uv : TEXCOORD0;
                        UNITY_FOG_COORDS(1) // Define [float4 fogCoord : TEXCOORD1;] if fog is enabled
                    };

                    sampler2D _MainTex;
                    float4 _MainTex_ST;

                    v2f vert (appdata v)
                    {
                        v2f o;
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                        o.color = v.color;

                        // Calcurate fog factor from o.vertex.z and set to o.fogCoord.x
                        // 1:base-color -> 0: fog-color
                        UNITY_TRANSFER_FOG(o,o.vertex);

                        return o;
                    }

                    fixed4 frag (v2f i) : SV_Target
                    {
                        half4 col = tex2D(_MainTex, i.uv) * i.color;

                        col.rgb = lerp(half3(1,1,1), col.rgb, col.a);

                        // Apply fog if enabled
                        UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(1,1,1,1)); // fog towards white due to our blend mode

                        return col;
                    }
                ENDCG
            }
        }
    }
}
