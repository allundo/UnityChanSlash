// ## CUSTOMIZED
// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Custom/Mobile/Diffuse-Additive-Trail-ClipY-Hologram"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [MainColor] _AdditiveColor ("Additive Color", Color) = (0, 0, 0, 1)
        _NoiseTex ("Noise", 2D) = "white" {}
        _ClipY("Clipping Y Plane", Range(0, 2.5)) = 0
        _TrailDir ("Trail Dir", Vector) = (0, 0, 0, 0)
        _HologramColor("Hologram Color", Color) = (0,1,0.4,0)
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 150

        CGPROGRAM
        #pragma surface surf Lambert noforwardadd vertex:vert
        #include "./CGIncludes/DitherTransparentFunctions.cginc"

        sampler2D _MainTex;
        sampler2D _NoiseTex;

        float4 _AdditiveColor;
        fixed4 _TrailDir;

        float _ClipY;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float4 screenPos;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            half3 trailObjDir = mul(unity_WorldToObject, _TrailDir);
            float weight = clamp(dot(v.normal, trailObjDir), 0, 1);
            float noise = 1 + tex2Dlod(_NoiseTex, v.texcoord).r * 0.5;
            fixed3 trail = trailObjDir * weight * noise;
            v.vertex.xyz += trail;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            clip(IN.worldPos.y - _ClipY);

            DitherClipping(IN.screenPos, _AdditiveColor.a, 1);

            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb + _AdditiveColor.rgb;
        }

        ENDCG

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

    Fallback "Mobile/VertexLit"
}
