using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BulletHeal : MonoBehaviour
{
    private Collider healCollider = default;
    protected IStatus status;

    protected virtual void Awake()
    {
        healCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        OnHitHeal(other);
    }

    protected virtual void OnActive()
    {
        healCollider.enabled = true;
    }

    protected IReactor OnHitHeal(Collider collider)
    {
        IMobReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return null;

        targetMob.OnHeal(status.attack);
        healCollider.enabled = false;

        return targetMob;
    }

    protected virtual void OnHitFinished()
    {
    }

}
