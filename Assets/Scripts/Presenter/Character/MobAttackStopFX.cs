using UnityEngine;
using DG.Tweening;

public class MobAttackStopFX : MobAttackFX
{
    [SerializeField] protected int fxStopFrame = -1;

    protected float fxStopSec;

    protected override void Start()
    {
        base.Start();
        fxStopSec = FrameToSec(fxStopFrame);
    }
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
            .InsertCallback(fxStopSec > fxStartSec ? fxStopSec : attackDuration, OnFXStop);
    }
}
