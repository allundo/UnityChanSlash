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

    // Set the end of attack sequence to stop playing effect in the case that Complete() is called.
    protected virtual void OnFXEnd()
    {
        attackFX.StopAndClear();
        attackSnd.StopEx();
    }

    public override Sequence AttackSequence(float attackDuration)
    {
        return base.AttackSequence(attackDuration)
            .InsertCallback(fxStartSec, OnFXStart)
            .AppendCallback(OnFXEnd);
    }
}
