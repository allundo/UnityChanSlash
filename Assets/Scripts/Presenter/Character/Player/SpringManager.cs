using UnityEngine;
using System;

public class SpringManager : MonoBehaviour
{
    [SerializeField] public SpringBone[] springBones;
    [SerializeField] public bool isWindActive = true;

    private RandomWind randomWind;

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
        Array.ForEach(springBones, bone => bone.UpdateSpring());
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
