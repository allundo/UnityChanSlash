using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class Attack : MonoBehaviour, IAttack
{
    private Collider attackCollider = default;
    protected MobStatus status;

    [SerializeField] protected float attackMultiplier = 1f;

    protected virtual void Awake()
    {
        attackCollider = GetComponent<Collider>();
        status = GetComponentInParent<MobStatus>();

        attackCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        OnHitAttack(other);
    }

    protected virtual void OnHitStart()
    {
        attackCollider.enabled = true;
    }

    protected virtual void OnHitAttack(Collider collider)
    {
        MobReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return;

        targetMob.OnDamage(status.Attack * attackMultiplier, status.dir);
    }

    protected virtual void OnHitFinished()
    {
        attackCollider.enabled = false;
    }

    public virtual Tween AttackSequence(float attackDuration)
    {
        return DOTween.Sequence()
            .AppendCallback(OnHitStart)
            .Join(DOVirtual.DelayedCall(attackDuration, OnHitFinished))
            .SetUpdate(false);
    }
}
