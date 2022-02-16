using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class Attack : MonoBehaviour, IAttack
{
    [SerializeField] protected AttackType attackType = default;
    [SerializeField] protected AttackAttr attackAttr = default;

    private Collider attackCollider = default;
    protected IStatus status;

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
        IReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return;

        targetMob.OnDamage(status.Attack * attackMultiplier, status.dir, attackType, attackAttr);
    }

    protected virtual void OnHitFinished()
    {
        attackCollider.enabled = false;
    }

    public virtual Tween AttackSequence(float attackDuration)
    {
        return DOTween.Sequence()
            .AppendCallback(OnHitStart)
            .AppendInterval(attackDuration)
            .AppendCallback(OnHitFinished)
            .SetUpdate(false);
    }
}
