using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;

public interface IStatus
{
    float Attack { get; }
    float Shield { get; }
    bool IsAlive { get; }

    Vector3 Position { get; }

    bool isOnGround { get; }
    bool isIced { get; }
    void SetIsIced(bool isIced);

    IDirection dir { get; }
    void SetDir(IDirection dir);

    IObservable<Unit> Active { get; }
    IReadOnlyReactiveProperty<float> Life { get; }
    IReadOnlyReactiveProperty<float> LifeMax { get; }
    float LifeRatio { get; }
    Vector3 corePos { get; }

    void Damage(float damage, AttackAttr attr = AttackAttr.None);

    void Heal(float heal);
    float CalcAttack(float attack, IDirection attackDir, AttackAttr attr = AttackAttr.None);

    void ResetStatus();

    void Activate();
    void Inactivate();

    MobStatus OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f);

    void SetPosition(Vector3 pos, IDirection dir = null);
    IStatus InitParam(MobParam param, float life = 0f);
}

public class MobStatus : SpawnObject<MobStatus>, IStatus
{
    protected MobParam param;

    protected virtual float FaceDamageMultiplier => param.faceDamageMultiplier;
    protected virtual float SideDamageMultiplier => param.sideDamageMultiplier;
    protected virtual float BackDamageMultiplier => param.backDamageMultiplier;
    protected virtual float RestDamageMultiplier => param.restDamageMultiplier;
    protected virtual float DefaultLifeMax => param.defaultLifeMax;

    protected static Dictionary<AttackAttr, float> attrDamageMultiplier
        = new Dictionary<AttackAttr, float>()
        {
            { AttackAttr.None,        1f },
            { AttackAttr.Fire,        1f },
            { AttackAttr.Ice,         1f },
            { AttackAttr.Thunder,     1f },
            { AttackAttr.Light,       1f },
            { AttackAttr.Dark,        1f },
        };

    public virtual float Attack
    {
        get { return param.attack; }
        set { param.attack = value; }
    }

    public virtual float Shield => param.shield;

    public bool isOnGround { get; protected set; }
    public bool isIced { get; protected set; }

    public void SetIsIced(bool isIced)
    {
        this.isIced = isIced;
    }

    protected virtual float ArmorMultiplier => param.armorMultiplier;

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

    protected bool isActive = false;

    public virtual Vector3 corePos => transform.position;

    protected virtual void Awake()
    {
        life = new ReactiveProperty<float>(0f);
        lifeMax = new ReactiveProperty<float>(0f);
    }

    public virtual void Damage(float damage, AttackAttr attr = AttackAttr.None)
    {
        life.Value -= damage;
    }

    public void Heal(float heal)
    {
        life.Value = Mathf.Min(life.Value + heal, lifeMax.Value);
    }

    public virtual float CalcAttack(float attack, IDirection attackDir, AttackAttr attr = AttackAttr.None)
    {
        return attack * ArmorMultiplier * GetDirMultiplier(isIced ? null : attackDir) * attrDamageMultiplier[attr];
    }

    protected float GetDirMultiplier(IDirection attackerDir)
    {
        if (attackerDir == null) return RestDamageMultiplier;

        if (attackerDir.IsInverse(dir))
        {
            return FaceDamageMultiplier;
        }

        if (attackerDir.IsSame(dir))
        {
            return BackDamageMultiplier;
        }

        return SideDamageMultiplier;
    }

    public virtual void ResetStatus()
    {
        life.Value = lifeMax.Value = DefaultLifeMax;
        isOnGround = param.isOnGround;
        isIced = false;
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
        ResetStatus();
        activeSubject.OnNext(Unit.Default);
    }

    public override void Inactivate()
    {
        if (!isActive) return;

        isActive = false;
        gameObject.SetActive(false);
    }

    public override MobStatus OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        SetPosition(pos, dir);
        Activate();
        return this;
    }

    public void SetPosition(Vector3 pos, IDirection dir = null)
    {
        transform.position = pos;

        this.dir = dir ?? MobStatus.defaultDir;
        transform.LookAt(transform.position + this.dir.LookAt);
    }

    public virtual IStatus InitParam(MobParam param, float life = 0f)
    {
        this.param = param;
        ResetStatus();

        if (life > 0f) this.life.Value = life;
        return this;
    }
}
