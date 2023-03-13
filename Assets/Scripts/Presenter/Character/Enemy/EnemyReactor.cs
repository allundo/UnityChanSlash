using UnityEngine;
using UniRx;
using System;

public interface IEnemyReactor : IMobReactor
{
    void OnSummoned();
    void OnTeleportEnd();
    bool IsTamed { get; }
    float ExpObtain { get; }
    EnemyType Type { get; }
}

[RequireComponent(typeof(EnemyEffect))]
public class EnemyReactor : MobReactor, IEnemyReactor
{
    private static readonly Vector3 OUT_OF_SCREEN = new Vector3(1024.0f, 0.0f, 1024.0f);

    protected IEnemyEffect enemyEffect;
    protected HologramFade hologramFade;
    protected IEnemyInput enemyInput;
    protected IEnemyStatus enemyStatus;
    private IDisposable inactiveNextFrame;
    private IDisposable lifeChange;
    private IDisposable targetEnemy;
    private bool isDestroying = false;

    public bool IsTamed => enemyStatus.isTamed;
    public float ExpObtain => enemyStatus.ExpObtain;
    public EnemyType Type => enemyStatus.type;

    protected LifeGaugeGenerator gaugeGenerator;

    protected override void Awake()
    {
        base.Awake();
        enemyEffect = effect as IEnemyEffect;
        enemyInput = input as IEnemyInput;
        enemyStatus = status as IEnemyStatus;

        gaugeGenerator = SpawnHandler.Instance.GetLifeGaugeGenerator();

        hologramFade = new HologramFade(transform);

        isDestroying = false;
    }

    protected override void Start()
    {
        enemyStatus.ActiveWithOption.Subscribe(option => OnActive(option)).AddTo(this);


    }

    protected override void OnLifeChange(float life)
    {
        if (CheckAlive(life) && !enemyStatus.IsTarget.Value) gaugeGenerator.Show(enemyStatus);
    }

    protected virtual bool CheckAlive(float life)
    {
        if (life > 0f) return true;
        input.InterruptDie();
        return false;
    }

    /// <summary>
    /// Before being dead, enemies must move out of player's detection on Minimap <br />
    /// since EnemySymbol inactivated OnTriggerExit of the detection.
    /// </summary>
    protected override void OnDead()
    {
        if (isDestroying) return;

        // Force TriggerExit from enemy detecting collider
        bodyCollider.enabled = true;
        transform.position = OUT_OF_SCREEN;

        // Wait for the TriggerExit event firing before inactivating the enemy
        inactiveNextFrame = Observable.NextFrame().Subscribe(_ => Inactivate());
    }

    private void Inactivate()
    {
        lifeChange?.Dispose();
        targetEnemy?.Dispose();
        input.ClearAll();
        bodyCollider.enabled = false;
        status.Inactivate();
    }

    public virtual void OnOutOfView()
    {
        if (isDestroying) return;

        effect.Disappear(() =>
        {
            input.ClearAll();
            map.ResetTile();
            OnDead();
        });
    }

    public override float Damage(IAttacker attacker, Attack.AttackData attackData)
    {
        var damage = base.Damage(attacker, attackData);

        if (attacker is PlayerShooter)
        {
            (attacker as PlayerShooter).IncMagic();
        }

        if (attacker is PlayerStatus)
        {
            (attacker as PlayerStatus).counter.IncAttack();
        }

        // Taming process
        if (attackData.attr == AttackAttr.Coin)
        {
            if (IsTamed)
            {
                ActiveMessageController.Instance.AlreadyTamed(enemyStatus);
            }
            else if (enemyStatus.TryTame())
            {
                ActiveMessageController.Instance.TameSucceeded(enemyStatus);
            }
        }
        else if (damage > 0.1f)
        {
            enemyStatus.CancelTamed();
        }

        return damage;
    }

    public override void Destroy()
    {
        // Stop all tweens before destroying
        input.ClearAll();
        effect.OnDestroyByReactor();

        inactiveNextFrame?.Dispose();

        bodyCollider.enabled = false;

        Destroy(gameObject);
        isDestroying = true;
    }

    public void OnSummoned()
    {
        enemyEffect.SummonFX();
    }

    public void OnTeleportEnd()
    {
        enemyEffect.OnTeleportEnd();
    }

    protected void Subscribe()
    {
        lifeChange?.Dispose();
        lifeChange = status.Life
            .SkipLatestValueOnSubscribe()
            .Subscribe(life => OnLifeChange(life))
            .AddTo(this);

        targetEnemy?.Dispose();
        targetEnemy = enemyStatus.IsTarget.Subscribe(isTarget =>
        {
            hologramFade.SetActive(isTarget);
            if (isTarget) gaugeGenerator.Hide(enemyStatus);
        })
        .AddTo(this);
    }

    protected virtual void OnActive(EnemyStatus.ActivateOption option)
    {
        Subscribe();

        enemyEffect.OnActive(option.fadeInDuration);
        map.OnActive();
        enemyInput.OnActive(option);
        bodyCollider.enabled = true;
    }

    protected override void OnActive() => OnActive(new EnemyStatus.ActivateOption());
}
