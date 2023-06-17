// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// CUSTOMIZED

// Simplified Additive Particle shader. Differences from regular Additive Particle one:
// - no Tint color
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask

Shader "Custom/Mobile/Particles/Additive"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
    }

    Category
    {
        Tags { "Queue"="AlphaTest-50" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha One
        Cull Off Lighting Off ZWrite Off
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
