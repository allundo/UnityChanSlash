using UnityEngine;
using UniRx;
using System.Collections.Generic;

public class MobStatus : MonoBehaviour
{
    protected MapUtil map;
    public Direction dir => map.dir;

    [SerializeField] public float FaceDamageMultiplier = 1.0f;
    [SerializeField] public float SideDamageMultiplier = 1.5f;
    [SerializeField] public float BackDamageMultiplier = 2.0f;
    [SerializeField] private float DefaultLifeMax = 10;

    protected List<Material> flashMaterials = new List<Material>();

    protected IReactiveProperty<float> life;
    public IReadOnlyReactiveProperty<float> Life => life;

    protected IReactiveProperty<float> lifeMax;
    public IReadOnlyReactiveProperty<float> LifeMax => lifeMax;

    public bool IsAlive => Life.Value > 0.0f;
    public float LifeRatio => life.Value / lifeMax.Value;

    public virtual float Attack => 1.0f;
    public virtual float Shield => 0.0f;

    protected virtual float ArmorMultiplier => 1.0f;

    protected virtual void Awake()
    {
        map = GetComponent<MapUtil>();
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

    public virtual float CalcAttack(float attack, Direction attackDir)
    {
        return attack * ArmorMultiplier * GetDirMultiplier(attackDir);
    }

    protected float GetDirMultiplier(Direction attackerDir)
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
        map.SetPosition();
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
