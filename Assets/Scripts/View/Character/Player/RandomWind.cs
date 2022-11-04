using UnityEngine;

public class RandomWind
{
    private Vector3 direction;
    private Quaternion deltaAngle;
    private int rotateFrames;
    private int frameCount;
    public bool isWindActive;

    public RandomWind(bool isWindActive = true)
    {
        this.isWindActive = isWindActive;
        direction = new Vector3(1f, 0f, 0f);
        ResetRotation();
    }

    private void ResetRotation()
    {
        frameCount = 0;
        rotateFrames = Random.Range(200, 2000);
        deltaAngle = Quaternion.Euler(0f, (float)Random.Range(-120, 120) / (float)rotateFrames, 0f);
    }

    public Vector3 GetNewWindForce()
    {
        direction = deltaAngle * direction;
        if (++frameCount == rotateFrames) ResetRotation();

        return isWindActive ? Mathf.PerlinNoise(Time.time, 0.0f) * 0.005f * direction : Vector3.zero;
    }
}
