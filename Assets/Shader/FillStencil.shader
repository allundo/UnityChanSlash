Shader "Custom/Stencil/FillStencilOnly"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        ZWrite Off
        Stencil
        {
            Ref 1 // Create hole
            Pass Replace
        }
        ColorMask 0

        Pass{}
    }
}
