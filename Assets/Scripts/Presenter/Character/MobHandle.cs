using UnityEngine;

[RequireComponent(typeof(HidePool))]
public class MobHandle : MonoBehaviour
{
    [SerializeField] private Collider handleCollider = default;
    private HidePool hidePool;


    private void Start()
    {
        hidePool = GetComponent<HidePool>();
        handleCollider.enabled = false;
    }

    public void OnHandleStart()
    {
        handleCollider.enabled = true;
    }

    public void OnHitHand(Collider collider)
    {
        DoorState targetDoor = collider.GetComponent<DoorState>();

        if (null == targetDoor) return;

        targetDoor.Handle();
        hidePool.Redraw();
    }

    public void OnHandleFinished()
    {
        handleCollider.enabled = false;
    }
}