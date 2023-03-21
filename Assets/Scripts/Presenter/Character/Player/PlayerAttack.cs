using UnityEngine;
using System;
using DG.Tweening;
using Random = UnityEngine.Random;

public interface IPlayerAttack : IMobAttack
{
    /// <summary>
    /// Returns critical attack sequence as a Tween
    /// </summary>
    /// <param name="criticalMultiplier">Multiplier applied to normal attack power.</param>
    /// <param name="expandScale">Expand scale applied to normal attack collider.</param>
    /// <returns></returns>
    Tween CriticalAttackSequence(float criticalMultiplier = 2.5f, float expandScale = 1.5f);
    void CompleteAttack();
}

public class PlayerAttack : MobAttackFX, IPlayerAttack
{
    [SerializeField] protected TweenParticle criticalFX = default;
    [SerializeField] protected AudioSource criticalSnd = default;
    [SerializeField] protected float minPitch = 1f;
    [SerializeField] protected float maxPitch = 1f;
    [SerializeField] protected float attackRatioR = 0f;
    [SerializeField] protected float attackRatioL = 0f;
    [SerializeField] protected EquipmentCategory category = EquipmentCategory.Knuckle;

    public static readonly float ATTACK_SPEED = Constants.PLAYER_ATTACK_SPEED;
    public static readonly float CRITICAL_SPEED = Constants.PLAYER_CRITICAL_SPEED;

    protected override float FrameToSec(int frame) => FrameToSec(frame, ATTACK_SPEED);

    protected override float currentAttack => attackMultiplier + playerStatus.AttackR * attackRatioR + playerStatus.AttackL * attackRatioL;

    protected PlayerStatus playerStatus;
    protected Sequence attackSequence;

    protected Func<bool> useEquip;

    protected override void Awake()
    {
        base.Awake();

        playerStatus = status as PlayerStatus;

        var itemInventory = ItemInventory.Instance;

        if (attackRatioR >= 1.0f)
        {
            useEquip = () => itemInventory.UseEquip(2);
        }
        else if (attackRatioL >= 1.0f)
        {
            useEquip = () => itemInventory.UseEquip(0);
        }
        else
        {
            useEquip = () => true;
        }
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

    protected override IReactor OnHitAttack(Collider collider)
    {
        var target = base.OnHitAttack(collider);
        if (target != null)
        {
            playerStatus.counter.IncAttack(category, attackMultiplier > defaultAttack);
            if (!useEquip()) CompleteAttack();
        }
        return target;
    }

    public override Sequence AttackSequence(float attackDuration)
    {
        attackSequence = DOTween.Sequence()
            .InsertCallback(fxStartSec, OnFXStart)
            .InsertCallback(startSec, OnHitStart)
            .InsertCallback(finishSec, OnHitFinished)
            .SetUpdate(false);

        return attackSequence;
    }

    public virtual Tween CriticalAttackSequence(float criticalMultiplier = 2.5f, float expandScale = 1.5f)
    {
        attackSequence = DOTween.Sequence()
            .AppendCallback(() => CriticalGain(criticalMultiplier, expandScale))
            .InsertCallback(FrameToSec(fxStartFrame, CRITICAL_SPEED), OnCriticalFXStart)
            .InsertCallback(FrameToSec(startFrame, CRITICAL_SPEED), OnHitStart)
            .InsertCallback(FrameToSec(finishFrame, CRITICAL_SPEED), OnHitFinished)
            .OnComplete(ResetAttackPower)
            .SetUpdate(false);

        return attackSequence;
    }

    protected void CriticalGain(float criticalMultiplier = 2.5f, float expandScale = 1.5f)
    {
        boxCollider.size *= expandScale;
        attackMultiplier *= criticalMultiplier;
    }

    public void CompleteAttack()
    {
        attackSequence?.Complete(true);
    }
}
