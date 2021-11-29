using UnityEngine;
using UniRx;
using System;

public class MobStatus : SpawnObject<MobStatus>
{
    [SerializeField] protected MobData data;
    [SerializeField] protected int dataIndex;
    protected MobParam param;

    protected virtual float FaceDamageMultiplier => param.faceDamageMultiplier;
    protected virtual float SideDamageMultiplier => param.sideDamageMultiplier;
    protected virtual float BackDamageMultiplier => param.backDamageMultiplier;
    protected virtual float RestDamageMultiplier => param.restDamageMultiplier;
    protected virtual float DefaultLifeMax => param.defaultLifeMax;

    public virtual float Attack => param.attack;
    public virtual float Shield => param.shield;

    protected virtual float ArmorMultiplier => param.armorMultiplier;

    public MapUtil map { get; protected set; }
    public IDirection dir => map.dir;

    protected IReactiveProperty<float> life;
    public IReadOnlyReactiveProperty<float> Life => life;

    protected IReactiveProperty<float> lifeMax;
    public IReadOnlyReactiveProperty<float> LifeMax => lifeMax;

    public bool IsAlive => Life.Value > 0.0f;
    public float LifeRatio => life.Value / lifeMax.Value;

    protected ISubject<float> onActive = new Subject<float>();
    public IObservable<float> OnActive => onActive;

    public bool isActive { get; protected set; } = false;

    protected virtual void Awake()
    {
        map = GetComponent<MapUtil>();

        param = data.Param(dataIndex);

        life = new ReactiveProperty<float>(DefaultLifeMax);
        lifeMax = new ReactiveProperty<float>(DefaultLifeMax);
    }

    protected virtual void Start()
    {
        Activate();
    }

    public void Damage(float damage)
    {
        life.Value -= damage;
    }

    public void Heal(float heal)
    {
        life.Value = Mathf.Min(life.Value + heal, lifeMax.Value);
    }

    public virtual float CalcAttack(float attack, IDirection attackDir)
    {
        return attack * ArmorMultiplier * GetDirMultiplier(attackDir);
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
        life.Value = LifeMax.Value;
    }
    public override MobStatus OnSpawn(Vector3 pos, IDirection dir = null, float duration = 0.5f)
    {
        Activate();
        map.SetPosition(pos, dir);
        return this;
    }

    public override void Activate()
    {
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);
        ResetStatus();
        onActive.OnNext(0.5f); // 0.5f: fade-in duration
    }

    public override void Inactivate()
    {
        if (!isActive) return;

        isActive = false;
        gameObject.SetActive(false);
    }
}
