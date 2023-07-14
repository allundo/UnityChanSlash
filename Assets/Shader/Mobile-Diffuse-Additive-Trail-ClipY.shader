Shader "Custom/Mobile/Diffuse-Additive-Trail-ClipY"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [MainColor] _AdditiveColor ("Additive Color", Color) = (0, 0, 0, 1)
        _NoiseTex ("Noise", 2D) = "white" {}
        _ClipY("Clipping Y Plane", Range(0, 2.5)) = 0
        _TrailDir ("Trail Dir", Vector) = (0, 0, 0, 0)
    }

    SubShader {
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
                    float4 vertex   : POSITION;
                    float2 uv       : TEXCOORD0;
                    float4 uvNoise  : TEXCOORD1;
                    float3 normal   : NORMAL;
                };

                struct v2f
                {
                    float4 vertex       : SV_POSITION;
                    float2 uv           : TEXCOORD0;
                    float4 screenPos    : TEXCOORD1;
                    float  worldPosY    : TEXCOORD2;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                sampler2D _NoiseTex;

                float4 _AdditiveColor;
                fixed4 _TrailDir;

                float _ClipY;

                v2f vert (appdata v)
                {
                    half3 trailObjDir = mul(unity_WorldToObject, _TrailDir);
                    float weight = clamp(dot(v.normal, trailObjDir), 0, 1);
                    float noise = 1 + tex2Dlod(_NoiseTex, v.uvNoise).r * 0.5;
                    fixed3 trail = trailObjDir * weight * noise;

                    v.vertex.xyz += trail;

                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	                o.screenPos = ComputeScreenPos(o.vertex);
                    o.worldPosY = mul(unity_ObjectToWorld, v.vertex).y;

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    clip(i.worldPosY - _ClipY);
                    DitherClipping(i.screenPos, _AdditiveColor.a, 1);

                    fixed4 col = tex2D(_MainTex, i.uv);

                    // Apply fog if enabled
                    col.rgb += _AdditiveColor.rgb;

                    return col;
                }
            ENDCG
        }
    }

    Fallback "Mobile/VertexLit"
}
