using UnityEngine;
using DG.Tweening;

public class MobAttackFX : MobAttack
{
    [SerializeField] protected ParticleSystem attackFX = default;
    [SerializeField] protected AudioSource attackSnd = default;
    [SerializeField] protected int fxStartFrame = 0;

    protected float fxStartSec;

    protected override void Start()
    {
        base.Start();
        fxStartSec = FrameToSec(fxStartFrame);
    }

    public override void OnDie()
    {
        attackFX.StopAndClear();
    }

    protected virtual void OnFXStart()
    {
        attackFX.PlayEx();
        attackSnd.PlayEx();
    }

    public override Sequence AttackSequence(float attackDuration)
    {
        return base.AttackSequence(attackDuration)
            .InsertCallback(fxStartSec, OnFXStart);
    }
}
