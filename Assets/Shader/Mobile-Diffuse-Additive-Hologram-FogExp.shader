// ## CUSTOMIZED
// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Custom/Mobile/Diffuse-Additive-Holgram-FogExp2"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [MainColor] _AdditiveColor ("Additive Color", Color) = (0, 0, 0, 1)
        _HologramColor("Hologram Color", Color) = (0,1,0.4,0)
    }

    CGINCLUDE
        #define FOG_EXP2
    ENDCG

    SubShader
    {
        Name "Main"
        Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
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
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 screenPos : TEXCOORD1;
                    UNITY_FOG_COORDS(2) // Define [float4 fogCoord : TEXCOORD2;] if fog is enabled
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _AdditiveColor;

                v2f vert (appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	                o.screenPos = ComputeScreenPos(o.vertex);

                    // Calcurate fog factor from o.vertex.z and set to o.fogCoord.x
                    // 1:base-color -> 0: fog-color
                    UNITY_TRANSFER_FOG(o,o.vertex);

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    DitherClipping(i.screenPos, _AdditiveColor.a, 1);

                    fixed4 col = tex2D(_MainTex, i.uv);

                    // Apply fog if enabled
                    UNITY_APPLY_FOG(i.fogCoord, col);
                    col.rgb += _AdditiveColor.rgb;

                    return col;
                }
            ENDCG
        }

        Pass
        {
            Name "Hologram"
            Tags { "RenderType"="Transparent" "Queue" = "Transparent+5" }

            Blend One One
            ZWrite Off
            ZTest Always
            LOD 150

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "./CGIncludes/DitherTransparentFunctions.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            fixed4 _HologramColor;
            float4 _AdditiveColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex.x -= 0.625 * o.vertex.w;  // 0.625 is magic number ... need to verify accurate value
	            o.screenPos = ComputeScreenPos(o.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex).xyz));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                DitherClipping(i.screenPos, (1 - saturate(dot(i.viewDir, i.normal))) * 1.5 * _HologramColor.a * _AdditiveColor.a);

                return _HologramColor;
            }
            ENDCG
        }
    }

    Fallback "Mobile/VertexLit"
}
