using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CubeDetectCount : MonoBehaviour
{
    private Collider cubeCollider;
    public int enterCount { get; private set; } = 0;
    public int exitCount { get; private set; } = 0;
    public bool stay { get; private set; } = false;

    void Awake()
    {
        cubeCollider = GetComponent<Collider>();
        cubeCollider.enabled = false;
        transform.position = new Vector3(0, 0, -5);
    }

    public void MoveIn()
    {
        transform.position = new Vector3(0, 0, -5);
    }

    public void MoveOut()
    {
        transform.position = new Vector3(0, 3, -5);
    }

    public void ColliderEnable(bool isEnabled)
    {
        cubeCollider.enabled = isEnabled;
    }

    public void OnTriggerEnter(Collider other)
    {
        enterCount++;
    }

    public void OnTriggerExit(Collider other)
    {
        exitCount++;
    }

    public void OnTriggerStay(Collider other)
    {
        stay = true;
    }

    public void LateUpdate()
    {
        stay = false;
    }
}
