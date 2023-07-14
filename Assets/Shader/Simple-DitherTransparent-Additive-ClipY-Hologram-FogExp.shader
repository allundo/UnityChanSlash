Shader "Custom/Simple/DitherTransparentAdditiveClipYHologramFogExp2"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        // This parameter can be editted only via Material.color in scripts
        [MainColor] _AdditiveColor("Additive Color", Color) = (0,0,0,1)

        _HologramColor("Hologram Color", Color) = (0,1,0.4,0)

        // Handled by script only
        _ClipY("Clipping Y Plane", Range(0, 2.5)) = 0

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Bump Scale", Range(0.0, 1.0)) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

        _OcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Emission Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
        #define _DITHER_ALPHA
        #define _ADDITIVE_COLOR
        #define _CLIP_Y
        #define FOG_EXP2
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma target 3.0

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_fragment _EMISSION

            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF

            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing

            #pragma vertex vertBase
            #pragma fragment fragBase
            #include "./CGIncludes/UnityStandardCoreForwardCustom.cginc"

            ENDCG
        }

        Pass
        {
            Name "FORWARD_ADDITIVE"
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One
            Fog { Color (0,0,0,0) }
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF

            #pragma multi_compile_fwdadd_fullshadows

            #pragma vertex vertAdd
            #pragma fragment fragAdd
            #include "./CGIncludes/UnityStandardCoreForwardCustom.cginc"

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
    FallBack "VertexLit"
}
