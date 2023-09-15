Shader "Custom/Simple/LightWeightTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
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
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.normal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);

                float diffuse = saturate(dot(i.normal, _WorldSpaceLightPos0.xyz));

                return diffuse * _LightColor0 * texColor;
            }
            ENDCG
        }
        
        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd_fullshadows

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.normal = UnityObjectToWorldNormal(v.normal);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);
                
                fixed4 texColor = tex2D(_MainTex, i.uv);

                float diffuse = saturate(dot(i.normal, normalize(_WorldSpaceLightPos0.xyz - i.worldPos.xyz)));

                return diffuse * texColor * _LightColor0 * attenuation;
            }
            ENDCG
        }
    }
}