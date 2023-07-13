Shader "Custom/Simple/MonoColorFogExp2"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        [HideInInspector] _MainTex("Albedo", 2D) = "white" {}

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        [HideInInspector] _EmissionColor("Emission Color", Color) = (0,0,0)
        [HideInInspector] _EmissionMap("Emission", 2D) = "white" {}
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
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

            #pragma shader_feature_fragment _EMISSION

            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma vertex vertBase
            #pragma fragment fragBase
            #include "UnityStandardCoreForward.cginc"

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
            #pragma multi_compile_fog

            #pragma vertex vertAdd
            #pragma fragment fragAdd
            #include "UnityStandardCoreForward.cginc"

            ENDCG
        }
    }
    FallBack "VertexLit"
}

