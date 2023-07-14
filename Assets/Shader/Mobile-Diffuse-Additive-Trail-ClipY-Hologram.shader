Shader "Custom/Mobile/Diffuse-Additive-Trail-ClipY-Hologram-FogExp2"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [MainColor] _AdditiveColor ("Additive Color", Color) = (0, 0, 0, 1)
        _NoiseTex ("Noise", 2D) = "white" {}
        _ClipY("Clipping Y Plane", Range(0, 2.5)) = 0
        _TrailDir ("Trail Dir", Vector) = (0, 0, 0, 0)
        _HologramColor("Hologram Color", Color) = (0,1,0.4,0)
    }

    CGINCLUDE
        #define FOG_EXP2
    ENDCG

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
                    float4 vertex   : POSITION;
                    float2 uv       : TEXCOORD0;
                    float4 uvNoise  : TEXCOORD1;
                    float3 normal   : NORMAL;
                };

                struct v2f
                {
                    float4 vertex       : SV_POSITION;
                    float2 uv           : TEXCOORD0;
                    float4 screenPos    : TEXCOORD1;
                    float  worldPosY    : TEXCOORD2;
                    UNITY_FOG_COORDS(3) // Define [float4 fogCoord : TEXCOORD2;] if fog is enabled
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                sampler2D _NoiseTex;

                float4 _AdditiveColor;
                fixed4 _TrailDir;

                float _ClipY;

                v2f vert (appdata v)
                {
                    half3 trailObjDir = mul(unity_WorldToObject, _TrailDir);
                    float weight = clamp(dot(v.normal, trailObjDir), 0, 1);
                    float noise = 1 + tex2Dlod(_NoiseTex, v.uvNoise).r * 0.5;
                    fixed3 trail = trailObjDir * weight * noise;

                    v.vertex.xyz += trail;

                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	                o.screenPos = ComputeScreenPos(o.vertex);
                    o.worldPosY = mul(unity_ObjectToWorld, v.vertex).y;

                    // Calcurate fog factor from o.vertex.z and set to o.fogCoord.x
                    // 1:base-color -> 0: fog-color
                    UNITY_TRANSFER_FOG(o,o.vertex);

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    clip(i.worldPosY - _ClipY);
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
