#ifndef ANGLE_FUNCTIONS
#define ANGLE_FUNCTIONS

#include "UnityCG.cginc"

float2 UVAngle(float2 uv, float angleRange)
{
    float angle = angleRange * UNITY_HALF_PI;

    float2 pivot = float2(0.5, 0.5);

    float cosAngle = cos(angle);
    float sinAngle = sin(angle);
    float2x2 rot = float2x2(cosAngle, -sinAngle, sinAngle, cosAngle);

    return mul(rot, uv - pivot) + pivot;
}

#endif // ANGLE_FUNCTIONS
