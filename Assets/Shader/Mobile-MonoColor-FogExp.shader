Shader "Custom/Mobile/Diffuse-MonoColor-FogExp2"
{
    Properties
    {
        [MainColor] _Color ("Color", Color) = (0, 0, 0, 1)
    }

    CGINCLUDE
        #define FOG_EXP2
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            Blend One Zero

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"
                #include "Lighting.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float3 normal : TEXCOORD0;
                    UNITY_FOG_COORDS(1) // Define [float4 fogCoord : TEXCOORD1;] if fog is enabled
                };

                float4 _Color;

                v2f vert (appdata v)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);

                    // Calcurate fog factor from o.vertex.z and set to o.fogCoord.x
                    // 1:base-color -> 0: fog-color
                    UNITY_TRANSFER_FOG(o,o.vertex);

                    o.normal = UnityObjectToWorldNormal(v.normal);
                    return o;
                }

                float4 frag (v2f i) : SV_Target
                {
                    fixed4 col = saturate(dot(i.normal, _WorldSpaceLightPos0.xyz)) * _LightColor0 * _Color;

                    // Apply fog if enabled
                    UNITY_APPLY_FOG(i.fogCoord, col);

                    return col;
                }

            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"
                #include "Lighting.cginc"
                #include "AutoLight.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float3 normal : TEXCOORD0;
                    float3 worldPos : TEXCOORD1;
                    UNITY_FOG_COORDS(2) // Define [float4 fogCoord : TEXCOORD2;] if fog is enabled
                };

                float4 _Color;

                v2f vert (appdata v)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);

                    // Calcurate fog factor from o.vertex.z and set to o.fogCoord.x
                    // 1:base-color -> 0: fog-color
                    UNITY_TRANSFER_FOG(o,o.vertex);

                    o.normal = UnityObjectToWorldNormal(v.normal);

                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                    return o;
                }

                float4 frag (v2f i) : SV_Target
                {
                    UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);

                    float diffuse = saturate(dot(i.normal, normalize(_WorldSpaceLightPos0.xyz - i.worldPos.xyz)));

                    fixed4 col = diffuse * _Color * _LightColor0 * attenuation;

                    // Apply fog if enabled
                    UNITY_APPLY_FOG(i.fogCoord, col);

                    return col;
                }

            ENDCG
        }
    }
}
