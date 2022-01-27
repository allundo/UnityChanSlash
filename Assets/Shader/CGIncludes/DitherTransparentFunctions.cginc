#ifndef DITHER_TRANSPARENT_FUNCTIONS
#define DITHER_TRANSPARENT_FUNCTIONS

#include "UnityCG.cginc"

sampler2D _DitherMaskLOD2D;

void DitherClipping(float4 screenPos, float alpha, int patternOffset = 0)
{
    // スクリーン座標 から ディザテクスチャ(DitherMaskLOD2D) へのマッピング
    // 4 x 4px 毎にテクスチャをマッピングする
    float2 dpos = screenPos.xy / screenPos.w * _ScreenParams.xy * 0.25;

    // ディザパターンを patternOffset[px] ずらす(半透明同士が重なった時の点滅防止)
    dpos.x += 0.25 * patternOffset;

    // DitherMaskLOD2D は Y 座標方向 (4*16px) に透明度が設定されている
    // _Color.a(0 〜 1) の値から Y のオフセット(0/16 〜 15/16 = 0.935)を決定
    // (スクリーン座標 / 4) の小数部分が 4 x 4 タイル内のオフセット(1/16 = 0.0625)に対応
    dpos.y = alpha * 0.9375 + frac(dpos.y) * 0.0625;

    // clip() : 値が 0 より小さければピクセルを破棄
    // ディザの値はAチャンネルに0,1で格納されている
    clip(tex2D(_DitherMaskLOD2D, dpos).a - 0.5);
}

#endif // DITHER_TRANSPARENT_FUNCTIONS
