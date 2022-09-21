using System;
using UniRx;
using UnityEngine;

public interface IEnemyStatus : IMobStatus
{
    EnemyType type { get; }
    EnemyStatus OnSpawn(Vector3 pos, IDirection dir, EnemyStatus.ActivateOption option);

    IObservable<EnemyStatus.ActivateOption> ActiveWithOption { get; }

    /// <summary>
    /// Try taming to the enemy.
    /// </summary>
    /// <param name="tamingPower">Value to multiply to taming probability</param>
    /// <returns>True if the taming succeeded.</returns>
    bool TryTame(float tamingPower = 1f);
    bool isTamed { get; }
    void CancelTamed();
}

public class EnemyStatus : MobStatus, IEnemyStatus
{
    public struct ActivateOption
    {
        public float fadeInDuration;
        public bool isSummoned;
        public float summoningDuration;
        public float icingFrames;
        public bool isHidden;
        public ActivateOption(float fadeInDuration = 0.5f, float icingFrames = 0f, bool isHidden = false, bool isSummoned = false, float summoningDuration = 120f)
        {
            this.fadeInDuration = fadeInDuration;
            this.icingFrames = icingFrames;
            this.isHidden = isHidden;
            this.isSummoned = isSummoned;
            this.summoningDuration = summoningDuration;
        }
    }

    protected ISubject<ActivateOption> activeWithOptionSubject = new BehaviorSubject<ActivateOption>(new ActivateOption());
    public IObservable<ActivateOption> ActiveWithOption => activeWithOptionSubject;

    public bool isTamed { get; protected set; } = false;
    public bool TryTame(float tamingPower = 1f)
    {
        isTamed = UnityEngine.Random.Range(0f, 1f) < enemyParam.tamingProbability * tamingPower;
        return isTamed;
    }

    public void CancelTamed() => isTamed = false;

    protected EnemyParam enemyParam;

    public EnemyType type => enemyParam.type;
    public override Vector3 corePos => enemyParam.enemyCore + transform.position;

    public override string CauseOfDeath(AttackType attackType = AttackType.None)
        => ResourceLoader.Instance.GetDeadCause(this.type, attackType);

    public override IStatus InitParam(Param param, StatusStoreData data = null)
    {
        enemyParam = param as EnemyParam;
        base.InitParam(param, data);

        if (data != null) isTamed = (data as EnemyStoreData).isTamed;

        return this;
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
