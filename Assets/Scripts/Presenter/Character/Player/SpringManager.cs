using UnityEngine;
using System;

public class SpringManager : MonoBehaviour
{
    protected static readonly int FRAME_RATE = 60;
    protected static readonly float FRAME_SEC_UNIT = 1f / (float)FRAME_RATE;

    public SpringBone[] springBones;

    private float DeltaTime => Time.deltaTime > 0f ? Time.deltaTime : 1f / FPS;
    private int FPS => Application.targetFrameRate > 0 ? Application.targetFrameRate : 60;

    void LateUpdate()
    {
        Array.ForEach(springBones, bone => bone.UpdateSpring(DeltaTime));
    }
}