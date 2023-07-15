Shader "Custom/Simple/DitherTransparentZWrite"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _GlossMapScale("Gloss Map Scale", Range(0.0, 1.0)) = 1.0
        _MetallicGlossMap("Metallic Gloss Map", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Bump Scale", Range(0.0, 1.0)) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Emission Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
        #define _DITHER_ALPHA
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300

        Pass
        {
            Name "ZWrite"
            Tags { "LightMode" = "ZBufferOnly" }

            ZWrite On
            ColorMask 0
            Lighting OFF

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "./CGIncludes/DitherTransparentFunctions.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 screenPos : TEXCOORD1;
            };

            half4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
	            o.screenPos = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                DitherClipping(i.screenPos, _Color.a);
                return fixed4(0,0,0,0);
            }

            ENDCG
        }

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma target 3.0

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_fragment _EMISSION
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature_local _PARALLAXMAP

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
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows

            #pragma vertex vertAdd
            #pragma fragment fragAdd
            #include "./CGIncludes/UnityStandardCoreForwardCustom.cginc"

            ENDCG
        }
    }

    FallBack "VertexLit"
}
