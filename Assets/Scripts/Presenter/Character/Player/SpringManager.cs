using UnityEngine;
using System;

public class SpringManager : MonoBehaviour
{
    [SerializeField] public SpringBone[] springBones;
    [SerializeField] public bool isWindActive = true;

    private RandomWind randomWind;

    void Awake()
    {
        randomWind = new RandomWind(isWindActive);
    }

    void LateUpdate()
    {
        Vector3 force = randomWind.GetNewWindForce();
        Array.ForEach(springBones, bone => bone.UpdateSpring(force));
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
