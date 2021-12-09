using UnityEngine;
using UniRx;

public class MobStatus : MonoBehaviour
{
    [SerializeField] protected MobData data;
    [SerializeField] protected int dataIndex;
    protected MobParam param;

    protected virtual float FaceDamageMultiplier => param.faceDamageMultiplier;
    protected virtual float SideDamageMultiplier => param.sideDamageMultiplier;
    protected virtual float BackDamageMultiplier => param.backDamageMultiplier;
    protected virtual float RestDamageMultiplier => param.restDamageMultiplier;
    protected virtual float DefaultLifeMax => param.defaultLifeMax;

    public virtual float Attack
    {
        get { return param.attack; }
        set { param.attack = value; }
    }

    public virtual float Shield => param.shield;

    public virtual bool IsOnGround => param.isOnGround;

    protected virtual float ArmorMultiplier => param.armorMultiplier;

    public MapUtil map { get; protected set; }
    public IDirection dir => map.dir;

    protected IReactiveProperty<float> life;
    public IReadOnlyReactiveProperty<float> Life => life;

    protected IReactiveProperty<float> lifeMax;
    public IReadOnlyReactiveProperty<float> LifeMax => lifeMax;

    public bool IsAlive => Life.Value > 0.0f;
    public float LifeRatio => life.Value / lifeMax.Value;

    public bool isActive { get; protected set; } = false;

    public void SetPosition(Vector3 pos, IDirection dir = null) => map.SetPosition(pos, dir);

    protected virtual void Awake()
    {
        map = GetComponent<MapUtil>();

        param = data.Param(dataIndex);

        life = new ReactiveProperty<float>(DefaultLifeMax);
        lifeMax = new ReactiveProperty<float>(DefaultLifeMax);
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

    public virtual void OnActive()
    {
        if (isActive) return;

        isActive = true;
        gameObject.SetActive(true);
        ResetStatus();
    }

    public void OnInactivate()
    {
        if (!isActive) return;

        isActive = false;
        gameObject.SetActive(false);
    }
}
