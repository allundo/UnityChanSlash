using UnityEngine;

public class RandomWind
{
    private Vector3 direction;
    private Quaternion angle;
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
        rotateFrames = UnityEngine.Random.Range(200, 2000);
        angle = Quaternion.Euler(0f, UnityEngine.Random.Range(-120f, 120f) / (float)rotateFrames, 0f);
    }

    public Vector3 GetNewWindForce()
    {
        direction = angle * direction;
        if (++frameCount == rotateFrames) ResetRotation();

        return isWindActive ? Mathf.PerlinNoise(Time.time, 0.0f) * 0.005f * direction : Vector3.zero;
    }
}
