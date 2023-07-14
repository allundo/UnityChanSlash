Shader "Custom/Mobile/Diffuse-Additive-FogExp2"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [MainColor] _AdditiveColor ("Additive Color", Color) = (0, 0, 0, 1)
    }

    CGINCLUDE
        #define FOG_EXP2
    ENDCG

    SubShader
    {
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
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 screenPos : TEXCOORD1;
                    UNITY_FOG_COORDS(2) // Define [float4 fogCoord : TEXCOORD2;] if fog is enabled
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _AdditiveColor;

                v2f vert (appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	                o.screenPos = ComputeScreenPos(o.vertex);

                    // Calcurate fog factor from o.vertex.z and set to o.fogCoord.x
                    // 1:base-color -> 0: fog-color
                    UNITY_TRANSFER_FOG(o,o.vertex);

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    DitherClipping(i.screenPos, _AdditiveColor.a, 1);

                    fixed4 col = tex2D(_MainTex, i.uv);

                    // Apply fog if enabled
                    UNITY_APPLY_FOG(i.fogCoord, col);
                    col.rgb += _AdditiveColor.rgb;

                    return col;
                }
            ENDCG
        }
    }

    Fallback "Mobile/VertexLit"
}
