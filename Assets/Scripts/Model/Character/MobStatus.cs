using UnityEngine;
using UniRx;

public class MobStatus : Status<MobData> { }

public class Status<T> : MonoBehaviour
    where T : MobData
{
    [SerializeField] protected T data;
    [SerializeField] protected int dataIndex;
    protected MobParam param;

    protected virtual float FaceDamageMultiplier => param.faceDamageMultiplier;
    protected virtual float SideDamageMultiplier => param.sideDamageMultiplier;
    protected virtual float BackDamageMultiplier => param.backDamageMultiplier;
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

    public virtual float CalcAttack(float attack, IDirection attackDir)
    {
        return attack * ArmorMultiplier * GetDirMultiplier(attackDir);
    }

    protected float GetDirMultiplier(IDirection attackerDir)
    {
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

    public virtual void Activate()
    {
        ResetStatus();
        transform.gameObject.SetActive(true);

        // TODO: Fade-in with custom shader
    }

    public virtual void Inactivate()
    {
        transform.gameObject.SetActive(false);

        // TODO: Fade-out with custom shader
    }
}
