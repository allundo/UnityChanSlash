// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Outline shader

// Material parameters
float4 _Color;
float4 _AdditiveColor;
float4 _LightColor0;
float _EdgeThickness = 1.0;
float4 _MainTex_ST;

// Textures
sampler2D _MainTex;
sampler2D _DitherMaskLOD2D;
// Structure from vertex shader to fragment shader
struct v2f
{
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float4 screenPos : TEXCOORD2;
};

// Float types
#define float_t  half
#define float2_t half2
#define float3_t half3
#define float4_t half4

// Outline thickness multiplier
#define INV_EDGE_THICKNESS_DIVISOR 0.00285
// Outline color parameters
#define SATURATION_FACTOR 0.6
#define BRIGHTNESS_FACTOR 0.8

// Vertex shader
v2f vert( appdata_base v )
{
	v2f o;
	o.uv = TRANSFORM_TEX( v.texcoord.xy, _MainTex );

	half4 projSpacePos = UnityObjectToClipPos( v.vertex );
	half4 projSpaceNormal = normalize( UnityObjectToClipPos( half4( v.normal, 0 ) ) );
	half4 scaledNormal = _EdgeThickness * INV_EDGE_THICKNESS_DIVISOR * projSpaceNormal; // * projSpacePos.w;

	scaledNormal.z += 0.00001;
	o.pos = projSpacePos + scaledNormal;

	o.screenPos = ComputeScreenPos(o.pos);

	return o;
}

// Fragment shader
float4 frag( v2f i ) : COLOR
{
	// Dither Transparent
	// Pixel clipping process
    float2 vpos = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy * 0.25;

    // ディザパターンを 1 px ずらす(半透明同士が重なった時の点滅防止)
    vpos.x += 0.25;

    vpos.y = _AdditiveColor.a * 0.9375 + frac(vpos.y) * 0.0625;

    clip(tex2D(_DitherMaskLOD2D, vpos).a - 0.5);


	float4_t diffuseMapColor = tex2D( _MainTex, i.uv );

	float_t maxChan = max( max( diffuseMapColor.r, diffuseMapColor.g ), diffuseMapColor.b );
	float4_t newMapColor = diffuseMapColor;

	maxChan -= ( 1.0 / 255.0 );
	float3_t lerpVals = saturate( ( newMapColor.rgb - float3( maxChan, maxChan, maxChan ) ) * 255.0 );
	newMapColor.rgb = lerp( SATURATION_FACTOR * newMapColor.rgb, newMapColor.rgb, lerpVals );

	return float4( BRIGHTNESS_FACTOR * newMapColor.rgb * diffuseMapColor.rgb + _AdditiveColor.rgb, diffuseMapColor.a ) * _Color * _LightColor0;
}
