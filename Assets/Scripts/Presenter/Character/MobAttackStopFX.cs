using UnityEngine;
using DG.Tweening;

public class MobAttackStopFX : MobAttackFX
{
    [SerializeField] protected int fxStopFrame = -1;

    protected virtual void OnFXStop()
    {
        attackFX.StopEmitting();
    }

    protected override IReactor OnHitAttack(Collider collider)
    {
        var target = base.OnHitAttack(collider);
        if (target != null) hitSubject.OnNext(target);
        return target;
    }

    public override Sequence AttackSequence(float attackDuration)
    {
        return base.AttackSequence(attackDuration)
            .InsertCallback(fxStopFrame > 0 ? FrameToSec(fxStopFrame) : attackDuration, OnFXStop);
    }
}
