using UnityEngine;
using DG.Tweening;

public abstract class AttackBehaviour : MonoBehaviour, IAttack
{
    public abstract Tween AttackSequence(float attackDuration);
}

[RequireComponent(typeof(Collider))]
public class Attack : AttackBehaviour
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

    protected virtual IReactor OnHitAttack(Collider collider)
    {
        IReactor targetMob = collider.GetComponent<MobReactor>();

        if (null == targetMob) return null;

        targetMob.Damage(status.attack * attackMultiplier, status.dir, attackType, attackAttr);

        return targetMob;
    }

    protected virtual void OnHitFinished()
    {
        attackCollider.enabled = false;
    }

    public override Tween AttackSequence(float attackDuration)
    {
        return DOTween.Sequence()
            .AppendCallback(OnHitStart)
            .AppendInterval(attackDuration)
            .AppendCallback(OnHitFinished)
            .SetUpdate(false);
    }
}
