using System;
using UniRx;
using UnityEngine;

public interface IEnemyStatus : IMobStatus
{
    EnemyType type { get; }
    EnemyStatus OnSpawn(Vector3 pos, IDirection dir, EnemyStatus.ActivateOption option);
}

public class EnemyStatus : MobStatus, IEnemyStatus
{
    public struct ActivateOption
    {
        public float fadeInDuration;
        public bool isSummoned;
        public float summoningDuration;
        public ActivateOption(float fadeInDuration = 0.5f, bool isSummoned = false, float summoningDuration = 120f)
        {
            this.fadeInDuration = fadeInDuration;
            this.isSummoned = isSummoned;
            this.summoningDuration = summoningDuration;
        }
    }

    protected ISubject<ActivateOption> activeWithOptionSubject = new BehaviorSubject<ActivateOption>(new ActivateOption());
    public IObservable<ActivateOption> ActiveWithOption => activeWithOptionSubject;

    protected EnemyParam enemyParam;

    public EnemyType type => enemyParam.type;
    public override Vector3 corePos => enemyParam.enemyCore + transform.position;

    public override IStatus InitParam(Param param, float life = 0f)
    {
        enemyParam = param as EnemyParam;
        return base.InitParam(param, life);
    }

    public virtual EnemyStatus OnSpawn(Vector3 pos, IDirection dir, ActivateOption option)
    {
        SetPosition(pos, dir);
        Activate(option);
        return this;
    }

    public override Status OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
        => OnSpawn(pos, dir, new ActivateOption(duration));

    public override void Activate() => Activate(new ActivateOption());

    protected void Activate(ActivateOption option)
    {
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);

        OnActive(option);
    }

    protected override void OnActive() => OnActive(new ActivateOption());
    protected virtual void OnActive(ActivateOption option)
    {
        // Don't reset status on activation
        // Set status by InitParam()

        activeWithOptionSubject.OnNext(option);
    }
}
