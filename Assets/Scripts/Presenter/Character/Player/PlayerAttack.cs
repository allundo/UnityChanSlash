using UnityEngine;
using DG.Tweening;

public interface IPlayerAttack : IMobAttack
{
    /// <summary>
    /// Returns critical attack sequence as a Tween
    /// </summary>
    /// <param name="additionalSpeed">Speed added to normal motion speed multiplier.</param>
    /// <param name="criticalMultiplier">Multiplier applied to normal attack power.</param>
    /// <param name="expandScale">Expand scale applied to normal attack collider.</param>
    /// <returns></returns>
    Tween CriticalAttackSequence(int additionalSpeed = 1, float criticalMultiplier = 2.5f, float expandScale = 1.5f);
}

public class PlayerAttack : MobAttack, IPlayerAttack
{
    [SerializeField] protected ParticleSystem criticalFX = default;
    [SerializeField] protected AudioSource criticalSnd = default;
    [SerializeField] protected float minPitch = 1f;
    [SerializeField] protected float maxPitch = 1f;

    public override void OnDie()
    {
        attackFX.StopAndClear();
        criticalFX.StopAndClear();
    }

    protected override void OnFXStart()
    {
        attackFX.PlayEx();
        attackSnd.SetPitch(Random.Range(0.7f, 1.3f));
        attackSnd.PlayEx();
    }

    protected virtual void OnCriticalFXStart()
    {
        criticalFX.PlayEx();
        criticalSnd.SetPitch(Random.Range(minPitch, maxPitch));
        criticalSnd.PlayEx();
    }

    public override Sequence AttackSequence(float attackDuration)
    {
        return DOTween.Sequence()
            .InsertCallback(fxStartSec, OnFXStart)
            .InsertCallback(startSec, OnHitStart)
            .InsertCallback(finishSec, OnHitFinished)
            .SetUpdate(false);
    }

    public virtual Tween CriticalAttackSequence(int additionalSpeed = 1, float criticalMultiplier = 2.5f, float expandScale = 1.5f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => CriticalGain(criticalMultiplier, expandScale))
            .InsertCallback(FrameToSec(fxStartFrame, speed + additionalSpeed), OnCriticalFXStart)
            .InsertCallback(FrameToSec(startFrame, speed + additionalSpeed), OnHitStart)
            .InsertCallback(FrameToSec(finishFrame, speed + additionalSpeed), OnHitFinished)
            .OnComplete(ResetAttackPower)
            .SetUpdate(false);
    }

    protected void CriticalGain(float criticalMultiplier = 2.5f, float expandScale = 1.5f)
    {
        boxCollider.size *= expandScale;
        attackMultiplier *= criticalMultiplier;
    }
}
