using UnityEngine;

public class BagCatcher : MonoBehaviour
{
    private SphereCollider rightHandCollider;

    void Awake()
    {
        rightHandCollider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var bag = other.GetComponent<YenBag>();
        if (bag == null) return;

        rightHandCollider.enabled = false;
        bag.CaughtBy(transform);
    }
}
