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
        IHandleStructure targetStructure = collider.GetComponent<HandleStructure>();

        if (null == targetStructure) return;

        targetStructure.Handle();
        hidePlateHandler.Redraw();
    }

    public void OnHandleFinished()
    {
        handleCollider.enabled = false;
    }
}