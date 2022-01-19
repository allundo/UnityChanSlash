#ifndef UTILITY_FUNCTIONS
#define UTILITY_FUNCTIONS

float curve(float c, float x)
{
    return x + x * (1.0 - x) * c;
}

float f_i_curve(float focus, float intensity, float x)
{
    float height = 1.0 - intensity;
    float c = (height - focus) / (1.0 - focus) / height;
    return curve(c, x);
}

float f_i_linear(float focus, float intensity, float x)
{
    return intensity / (1.0 - focus) * (x - focus) - intensity + 1.0;
}

float f_i_curve_linear(float focus, float intensity, float x)
{
    return lerp(f_i_curve(focus, intensity, x), f_i_linear(focus, intensity, x), step(intensity, x));
}

float CurveLinearGradation(float x, float pos, float focus, float intensity)
{
    return lerp(f_i_curve_linear(focus, intensity, x / pos), f_i_curve_linear(focus, intensity, (1.0 - x) / (1.0 - pos)), step(pos, x));
}

float CurveLinearGradation(float2 uv, float pos, float focus, float intensity, float angleRange)
{
    float gradSample = saturate(UVAngle(uv, angleRange).x);
    return CurveLinearGradation(gradSample, pos, focus, intensity);
}

#endif // UTILITY_FUNCTIONS
