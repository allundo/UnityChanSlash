using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MobStatus))]
public class MobHandle : MonoBehaviour
{
    [SerializeField] private Collider handleCollider = default;


    private void Start()
    {
        handleCollider.enabled = false;
    }

    public void OnHandleStart()
    {
        handleCollider.enabled = true;
    }

    public void OnHitHand(Collider collider)
    {
        DoorControl targetDoor = collider.GetComponent<DoorControl>();

        if (null == targetDoor) return;

        targetDoor.Handle();
    }

    public void OnHandleFinished()
    {
        handleCollider.enabled = false;
    }
}