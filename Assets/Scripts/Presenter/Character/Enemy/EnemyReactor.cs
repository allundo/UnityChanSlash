using UnityEngine;
using UniRx;
using System;

public interface IEnemyReactor : IMobReactor
{
    void OnSummoned();
    void OnTeleportEnd();
}

[RequireComponent(typeof(EnemyEffect))]
public class EnemyReactor : MobReactor, IEnemyReactor
{
    private static readonly Vector3 OUT_OF_SCREEN = new Vector3(1024.0f, 0.0f, 1024.0f);

    protected IEnemyEffect enemyEffect;
    protected IEnemyInput enemyInput;
    private IDisposable inactiveNextFrame;

    protected override void Awake()
    {
        base.Awake();
        enemyEffect = effect as IEnemyEffect;
        enemyInput = input as IEnemyInput;
    }

    protected override void Start()
    {
        status.Life
            .SkipLatestValueOnSubscribe()
            .Subscribe(life => OnLifeChange(life))
            .AddTo(this);

        (status as EnemyStatus).ActiveWithOption.Subscribe(option => OnActive(option)).AddTo(this);
    }

    /// <summary>
    /// Before being dead, enemies must move out of player's detection on Minimap <br />
    /// since EnemySymbol inactivated OnTriggerExit of the detection.
    /// </summary>
    protected override void OnDead()
    {
        // Force TriggerExit from enemy detecting collider
        bodyCollider.enabled = true;
        transform.position = OUT_OF_SCREEN;

        // Wait for the TriggerExit event firing before inactivating the enemy
        inactiveNextFrame = Observable.NextFrame().Subscribe(_ => Inactivate());
    }

    private void Inactivate()
    {
        input.ClearAll();
        bodyCollider.enabled = false;
        status.Inactivate();
    }

    public void Disappear()
    {
        effect.Disappear(() =>
        {
            input.ClearAll();
            map.ResetTile();
            OnDead();
        });
    }

    public override void Destroy()
    {
        // Stop all tweens before destroying
        input.ClearAll();
        effect.OnDestroyByReactor();

        inactiveNextFrame?.Dispose();

        bodyCollider.enabled = false;
        map.ResetTile();

        Destroy(gameObject);
    }

    public void OnSummoned()
    {
        enemyEffect.SummonFX();
    }

    public void OnTeleportEnd()
    {
        enemyEffect.OnTeleportEnd();
    }

    protected void OnActive(EnemyStatus.ActivateOption option)
    {
        enemyEffect.OnActive(option.fadeInDuration);
        map.OnActive();
        enemyInput.OnActive(option);
        bodyCollider.enabled = true;
    }

    protected override void OnActive() => OnActive(new EnemyStatus.ActivateOption());
}
