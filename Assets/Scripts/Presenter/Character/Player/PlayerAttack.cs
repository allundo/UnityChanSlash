using UnityEngine;
using DG.Tweening;

public interface IPlayerAttack : IMobAttack
{
    /// <summary>
    /// Returns critical attack sequence as a Tween
    /// </summary>
    /// <param name="criticalMultiplier">Multiplier applied to normal attack power.</param>
    /// <param name="expandScale">Expand scale applied to normal attack collider.</param>
    /// <returns></returns>
    Tween CriticalAttackSequence(float criticalMultiplier = 2.5f, float expandScale = 1.5f);
}

public class PlayerAttack : MobAttackFX, IPlayerAttack
{
    [SerializeField] protected TweenParticle criticalFX = default;
    [SerializeField] protected AudioSource criticalSnd = default;
    [SerializeField] protected float minPitch = 1f;
    [SerializeField] protected float maxPitch = 1f;
    [SerializeField] protected float attackRatioR = 0f;
    [SerializeField] protected float attackRatioL = 0f;

    public static readonly float ATTACK_SPEED = Constants.PLAYER_ATTACK_SPEED;
    public static readonly float CRITICAL_SPEED = Constants.PLAYER_CRITICAL_SPEED;

    protected override float FrameToSec(int frame) => FrameToSec(frame, ATTACK_SPEED);

    protected override float currentAttack => attackMultiplier + playerStatus.AttackR * attackRatioR + playerStatus.AttackL * attackRatioL;

    protected PlayerStatus playerStatus;

    protected override void Awake()
    {
        base.Awake();
        playerStatus = status as PlayerStatus;
    }

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
        criticalFX.Play();
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

    public virtual Tween CriticalAttackSequence(float criticalMultiplier = 2.5f, float expandScale = 1.5f)
    {
        return DOTween.Sequence()
            .AppendCallback(() => CriticalGain(criticalMultiplier, expandScale))
            .InsertCallback(FrameToSec(fxStartFrame, CRITICAL_SPEED), OnCriticalFXStart)
            .InsertCallback(FrameToSec(startFrame, CRITICAL_SPEED), OnHitStart)
            .InsertCallback(FrameToSec(finishFrame, CRITICAL_SPEED), OnHitFinished)
            .OnComplete(ResetAttackPower)
            .SetUpdate(false);
    }

    protected void CriticalGain(float criticalMultiplier = 2.5f, float expandScale = 1.5f)
    {
        boxCollider.size *= expandScale;
        attackMultiplier *= criticalMultiplier;
    }
}
