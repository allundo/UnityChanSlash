using UnityEngine;

[RequireComponent(typeof(HidePlateHandler))]
public class MobHandle : MonoBehaviour
{
    [SerializeField] private Collider handleCollider = default;
    private HidePlateHandler hidePlateHandler;

    private void Start()
    {
        hidePlateHandler = GetComponent<HidePlateHandler>();
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
        hidePlateHandler.Redraw();
    }

    public void OnHandleFinished()
    {
        handleCollider.enabled = false;
    }
}