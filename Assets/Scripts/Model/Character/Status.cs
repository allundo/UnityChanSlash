using UnityEngine;
using UniRx;
using System;

public interface IStatus : IAttacker
{
    GameObject gameObject { get; }
    bool isActive { get; }

    bool IsAlive { get; }

    Vector3 Position { get; }

    void SetDir(IDirection dir);

    IObservable<Unit> Active { get; }
    IReadOnlyReactiveProperty<float> Life { get; }
    IReadOnlyReactiveProperty<float> LifeMax { get; }
    float LifeRatio { get; }
    bool IsLifeMax { get; }

    void LifeChange(float diff, AttackAttr attr = AttackAttr.None);

    float MagicMultiplier { get; }

    void ResetStatus(float life = 0f);

    void Activate();
    void Inactivate();

    Status OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f);

    void SetPosition(Vector3 pos, IDirection dir = null);
    IStatus InitParam(Param param, StatusStoreData data = null);
}

public class Status : SpawnObject<Status>, IStatus
{
    protected Param param;

    public virtual string Name => param.name;

    public virtual string CauseOfDeath(AttackType type = AttackType.None) => Name + "にやられた";

    public float attack { get; protected set; }

    public virtual float MagicMultiplier => 1f;

    public bool isOnGround { get; protected set; }

    public IDirection dir { get; protected set; }
    public void SetDir(IDirection dir) => this.dir = dir;
    private static readonly IDirection defaultDir = new South();

    public Vector3 Position => transform.position;

    protected IReactiveProperty<float> life;
    public IReadOnlyReactiveProperty<float> Life => life;

    protected IReactiveProperty<float> lifeMax;
    public IReadOnlyReactiveProperty<float> LifeMax => lifeMax;

    protected ISubject<Unit> activeSubject = new BehaviorSubject<Unit>(Unit.Default);
    public IObservable<Unit> Active => activeSubject;

    public bool IsAlive => Life.Value > 0.0f;
    public float LifeRatio => life.Value / lifeMax.Value;
    public bool IsLifeMax => life.Value == lifeMax.Value;

    public bool isActive { get; protected set; } = false;

    protected virtual void Awake()
    {
        life = new ReactiveProperty<float>(0f);
        lifeMax = new ReactiveProperty<float>(0f);
    }

    public virtual void LifeChange(float diff, AttackAttr attr = AttackAttr.None)
    {
        life.Value = Mathf.Clamp(life.Value + diff, 0f, lifeMax.Value);
    }

    public virtual void ResetStatus(float life = 0f)
    {
        lifeMax.Value = param.defaultLifeMax;
        this.life.Value = life == 0f ? lifeMax.Value : life;
    }

    public override void Activate()
    {
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);

        OnActive();
    }

    protected virtual void OnActive()
    {
        activeSubject.OnNext(Unit.Default);
    }

    public override void Inactivate()
    {
        if (!isActive) return;

        isActive = false;
        gameObject.SetActive(false);
    }

    public override Status OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        SetPosition(pos, dir);
        Activate();
        return this;
    }

    public void SetPosition(Vector3 pos, IDirection dir = null)
    {
        transform.position = pos;

        this.dir = dir ?? Status.defaultDir;
        transform.rotation = this.dir.Rotate;
    }

    public virtual IStatus InitParam(Param param, StatusStoreData data = null)
    {
        this.param = param;

        attack = param.attack;
        ResetStatus();
        return this;
    }
}
