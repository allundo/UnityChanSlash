using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DetectSphere : MonoBehaviour
{
    private Collider cubeCollider;
    void Awake()
    {
        cubeCollider = GetComponent<Collider>();
        cubeCollider.enabled = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        var sphere = other.GetComponent<TestSphere>();
        if (sphere != null)
        {
            sphere.isDetected = true;
        }
    }
}
