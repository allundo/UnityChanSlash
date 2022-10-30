using UnityEngine;
using DG.Tweening;

public abstract class AttackBehaviour : MonoBehaviour, IAttack
{
    public abstract Sequence AttackSequence(float attackDuration);
}

[RequireComponent(typeof(Collider))]
public class Attack : AttackBehaviour
{
    public struct AttackData
    {
        public AttackType type;
        public AttackAttr attr;
        public float multiplier;

        public AttackData(float multiplier = 1f, AttackType type = AttackType.Smash, AttackAttr attr = AttackAttr.None)
        {
            this.multiplier = multiplier;
            this.type = type;
            this.attr = attr;
        }
    }

    [SerializeField] protected AttackType attackType = default;
    [SerializeField] protected AttackAttr attackAttr = default;
    [SerializeField] protected float attackMultiplier = 1f;

    protected Collider attackCollider = default;
    protected IStatus status;

    protected virtual float currentAttack => attackMultiplier;

    protected virtual void Awake()
    {
        attackCollider = GetComponent<Collider>();
        status = GetComponentInParent<Status>();

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

        targetMob.Damage(status, new AttackData(currentAttack, attackType, attackAttr));

        return targetMob;
    }

    protected virtual void OnHitFinished()
    {
        attackCollider.enabled = false;
    }

    public override Sequence AttackSequence(float attackDuration)
    {
        return DOTween.Sequence()
            .AppendCallback(OnHitStart)
            .AppendInterval(attackDuration)
            .AppendCallback(OnHitFinished)
            .SetUpdate(false);
    }
}
