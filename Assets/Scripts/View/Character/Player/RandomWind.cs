using UnityEngine;
using System;

public class RandomWind
{
    private SpringBone[] springBones;
    private Vector3 direction;
    private Quaternion angle;
    private int rotateFrames;
    private int frameCount;
    public bool isWindActive;

    public RandomWind(SpringBone[] springBones, bool isWindActive = true)
    {
        this.springBones = springBones;
        this.isWindActive = isWindActive;
        direction = new Vector3(1f, 0f, 0f);
        ResetRotation();
    }

    private void UpdateDirection()
    {
        direction = angle * direction;
        if (++frameCount == rotateFrames) ResetRotation();
    }
    private void ResetRotation()
    {
        frameCount = 0;
        rotateFrames = UnityEngine.Random.Range(200, 2000);
        angle = Quaternion.Euler(0f, UnityEngine.Random.Range(-120f, 120f) / (float)rotateFrames, 0f);
    }

    public void UpdateSpringForce()
    {
        Vector3 force = isWindActive ? Mathf.PerlinNoise(Time.time, 0.0f) * 0.005f * direction : Vector3.zero;

        Array.ForEach(springBones, bone => bone.springForce = force);

        UpdateDirection();
    }
}