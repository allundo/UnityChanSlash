using System;
using UniRx;
using UnityEngine;

public interface IEnemyStatus : IMobStatus
{
    EnemyType type { get; }
    EnemyStatus OnSpawn(Vector3 pos, IDirection dir, EnemyStatus.ActivateOption option);

    float ExpObtain { get; }

    IObservable<EnemyStatus.ActivateOption> ActiveWithOption { get; }

    IReadOnlyReactiveProperty<bool> IsTarget { get; }
    void SetTarget(bool isTarget);
    string TargetName { get; }

    /// <summary>
    /// Try taming to the enemy.
    /// </summary>
    /// <param name="tamingPower">Value to multiply to taming probability</param>
    /// <returns>True if the taming succeeded.</returns>
    bool TryTame(float tamingPower = 1f);
    bool isTamed { get; }
    void CancelTamed();
}

public class EnemyShooter : Shooter
{
    private EnemyType type;
    public override string CauseOfDeath(AttackType attackType = AttackType.None)
        => ResourceLoader.Instance.GetDeadCause(this.type, attackType);

    public EnemyShooter(float attack, IEnemyStatus status) : base(attack, status)
    {
        type = status.type;
    }
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
        public bool isSleeping;
        public ActivateOption(float fadeInDuration = 0.5f, float icingFrames = 0f, bool isHidden = false, bool isSummoned = false, float summoningDuration = 120f)
        {
            this.fadeInDuration = fadeInDuration;
            this.icingFrames = icingFrames;
            this.isHidden = isHidden;
            this.isSummoned = isSummoned;
            this.summoningDuration = summoningDuration;
            this.isSleeping = false;
        }

        public ActivateOption(DataStoreAgent.EnemyData data)
        {
            this.fadeInDuration = 0.25f;
            this.icingFrames = data.icingFrames;
            this.isHidden = data.isHidden;
            this.isSummoned = false;
            this.summoningDuration = 0f;
            this.isSleeping = data.life == 0f;
        }
    }

    protected ISubject<ActivateOption> activeWithOptionSubject = new BehaviorSubject<ActivateOption>(new ActivateOption());
    public IObservable<ActivateOption> ActiveWithOption => activeWithOptionSubject;
    public string TargetName => $"{Name}\nLv{level + 1}";

    public IReadOnlyReactiveProperty<bool> IsTarget => isTarget;
    private IReactiveProperty<bool> isTarget = new ReactiveProperty<bool>(false);
    public void SetTarget(bool isTarget)
    {
        this.isTarget.Value = isTarget;
    }

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

    public float ExpObtain => enemyParam.baseExp * (1f + 0.4f * level);

    public override string CauseOfDeath(AttackType attackType = AttackType.None)
        => ResourceLoader.Instance.GetDeadCause(this.type, attackType);

    public override void ResetStatus(float life = 0f)
    {
        base.ResetStatus(life);
        isTamed = false;
    }

    public override IStatus InitParam(Param param, StatusStoreData data = null)
        => InitParam(param as EnemyParam, data as EnemyStoreData);

    protected virtual IEnemyStatus InitParam(EnemyParam param, EnemyStoreData data)
    {
        data = data ?? new EnemyStoreData(Util.GetEnemyLevel());

        this.enemyParam = param;
        levelGain = ResourceLoader.Instance.enemyLevelGainData.Param((int)param.gainType);

        base.InitParam(param, data);
        isTamed = data.isTamed;
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
