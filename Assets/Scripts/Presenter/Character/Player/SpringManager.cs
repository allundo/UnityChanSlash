using UnityEngine;
using System;

public class SpringManager : MonoBehaviour
{
    [SerializeField] public SpringBone[] springBones;
    [SerializeField] public bool isWindActive = true;

    private RandomWind randomWind;

    protected static readonly float FRAME_SEC_UNIT = 1f / (float)Constants.FRAME_RATE;

    void Awake()
    {
        randomWind = new RandomWind(springBones, isWindActive);
    }

    void Update()
    {
        randomWind.UpdateSpringForce();
    }

    void LateUpdate()
    {
        var deltaTime = Time.timeScale > 0 ? Time.deltaTime : FRAME_SEC_UNIT;
        Array.ForEach(springBones, bone => bone.UpdateSpring(deltaTime));
    }

    public void Pause()
    {
        randomWind.isWindActive = false;
    }

    public void Resume()
    {
        randomWind.isWindActive = isWindActive;
    }
}
