using UnityEngine;

[RequireComponent(typeof(HidePlateUpdater))]
public class MobHandle : MonoBehaviour
{
    [SerializeField] private Collider handleCollider = default;
    private HidePlateUpdater hidePool;


    private void Start()
    {
        hidePool = GetComponent<HidePlateUpdater>();
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