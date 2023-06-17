// ## CUSTOMIZED
// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Additive Particle shader. Differences from regular Additive Particle one:
// - no Tint color
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask

Shader "Custom/Mobile/UI/Particles/Alpha Blended"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
    }

    Category
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off Lighting Off ZWrite Off
        ZTest [unity_GUIZTestMode]
        Fog { Mode Off }

        BindChannels
        {
            Bind "Color", color
            Bind "Vertex", vertex
            Bind "TexCoord", texcoord
        }

        SubShader
        {
            Pass
            {
                SetTexture [_MainTex]
                {
                    combine texture * primary
                }
            }
        }
    }
}
