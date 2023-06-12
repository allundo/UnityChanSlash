#ifndef GRADATION_FUNCTIONS
#define GRADATION_FUNCTIONS

#include "UnityCG.cginc"
#include "AngleFunctions.cginc"

// Applies value:a if x < border, else applies value:b
// lerp(a, b, step(border, x));

float f_linear(float focus, float x)
{
    return saturate((x - focus) / (1.0 - focus));
}

float f_w_linear(float focus, float width, float x)
{
    float range = 1.0 - width;
    return lerp(f_linear(focus, x / range), 1.0, step(range, x));
}

float sigmoid8(float x)
{
    return 1.0 / (1.0 + exp(- 8.0 * x));
}

float ease_sigmoid8(float x)
{
    float SIGMOID8_MAX = 0.88079707797; // sigmoid8(0.25)
    float SIGMOID8_MIN = 0.00247262315; // sigmoid8(-0.75)
    return (sigmoid8(x - 0.75) - SIGMOID8_MIN) / SIGMOID8_MAX;
}

float f_sigmoid(float focus, float x)
{
    return saturate(ease_sigmoid8((x - focus) / (1.0 - focus)));
}

float f_w_sigmoid(float focus, float width, float x)
{
    float range = 1.0 - width;
    return lerp(f_sigmoid(focus, x / range), 1.0, step(range, x));
}

float LinearGradation(float x, float pos, float focus, float width)
{
    return lerp(f_w_linear(focus, width, x / pos), f_w_linear(focus, width, (1.0 - x) / (1.0 - pos)), step(pos, x));
}

float LinearGradation(float2 uv, float pos, float focus, float width, float angleRange)
{
    float gradSample = saturate(UVAngle(uv, angleRange).x);
    return LinearGradation(gradSample, pos, focus, width);
}

float SigmoidGradation(float x, float pos, float focus, float width)
{
    return lerp(f_w_sigmoid(focus, width, x / pos), f_w_sigmoid(focus, width, (1.0 - x) / (1.0 - pos)), step(pos, x));
}

float SigmoidGradation(float2 uv, float pos, float focus, float width, float angleRange)
{
    float gradSample = saturate(UVAngle(uv, angleRange).x);
    return SigmoidGradation(gradSample, pos, focus, width);
}

float RoundGradation(float2 uv, float2 pos, float focus, float radius)
{
    return f_w_linear(focus, radius, 1.0 - distance(uv, pos) * 2.0);
}

float EllipseGradation(float2 uv, float2 pos, float focus, float radius, float ratio, float angleRange)
{
    float2 gradSampleUV = UVAngle(uv, angleRange);
    float2 ratioX = float2(ratio, 1.0);

    return f_w_linear(focus, radius, 1.0 - distance(gradSampleUV / ratioX, pos / ratioX) * 2.0);
}

#endif // GRADATION_FUNCTIONS
