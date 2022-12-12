using System;
using UniRx;
using UnityEngine;

public interface IEnemyStatus : IMobStatus
{
    EnemyType type { get; }
    EnemyStatus OnSpawn(Vector3 pos, IDirection dir, EnemyStatus.ActivateOption option);

    float ExpObtain { get; }

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

public class Shooter : Attacker, IAttacker
{
    private EnemyType type;
    public override string CauseOfDeath(AttackType attackType = AttackType.None)
        => ResourceLoader.Instance.GetDeadCause(this.type, attackType);

    public static IAttacker New(float attack, IStatus status)
    {
        if (status is IEnemyStatus) return new Shooter(attack, status as IEnemyStatus);
        if (status is PlayerStatus) return status;
        return new Attacker(attack, status.dir, status.Name);
    }

    private Shooter(float attack, IEnemyStatus status) : base(attack, status.dir, status.Name)
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
    public override string Name => $"{param.name} Lv{level + 1}";

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

    public override void ResetStatus()
    {
        base.ResetStatus();
        isTamed = false;
    }

    public override IStatus InitParam(Param param, StatusStoreData data = null)
        => InitParam(param as EnemyParam, data as EnemyStoreData);

    private IEnemyStatus InitParam(EnemyParam param, EnemyStoreData data)
    {
        this.enemyParam = param;
        levelGain = ResourceLoader.Instance.levelGainData.Param((int)param.gainType);

        data = data ?? new EnemyStoreData(param.defaultLifeMax, 0, false);

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
