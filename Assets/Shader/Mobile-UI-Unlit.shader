Shader "Custom/Mobile/UI/Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Fog { Mode Off }

        Pass
        {
            SetTexture [_MainTex]
            {
                combine texture * primary
            }
        }
    }
}
