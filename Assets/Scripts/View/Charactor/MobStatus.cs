using UnityEngine;
using UniRx;
using System.Collections.Generic;
using DG.Tweening;

[RequireComponent(typeof(MobCommander))]
public class MobStatus : MonoBehaviour
{
    protected MobCommander commander = default;
    protected Direction dir => commander.map.dir;

    protected MapUtil map;

    [SerializeField] public float FaceDamageMultiplier = 1.0f;
    [SerializeField] public float SideDamageMultiplier = 1.5f;
    [SerializeField] public float BackDamageMultiplier = 2.0f;

    protected List<Material> flashMaterials = new List<Material>();

    protected float life
    {
        get
        {
            return Life.Value;
        }
        set
        {
            Life.Value = value;
        }
    }

    public IReactiveProperty<float> Life = new ReactiveProperty<float>(0.0f);

    public bool IsAlive => life > 0.0f;

    [SerializeField] public float LifeMax = 10;

    public virtual int Attack => 1;

    protected virtual float Shield(Direction attackDir) => 0.0f;

    protected virtual float ArmorMultiplier => 1.0f;

    protected virtual void Awake()
    {
        map = GetComponent<MapUtil>();
        commander = GetComponent<MobCommander>();
    }

    protected virtual void Start()
    {
        StoreMaterialColors();
        Activate();
    }

    protected void StoreMaterialColors()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.HasProperty("_AdditiveColor"))
                {
                    flashMaterials.Add(mat);
                }
            }
        }
    }

    protected void DamageFlash(float damage)
    {
        float rate = Mathf.Clamp(damage / LifeMax, 0.01f, 1.0f);

        foreach (Material mat in flashMaterials)
        {
            Sequence flash = DOTween.Sequence().Append(mat.DOColor(Color.white, 0.02f));

            if (rate > 0.1f)
            {
                flash.Append(mat.DOColor(Color.black, 0.02f));
                flash.Append(mat.DOColor(Color.red, 0.02f));
            }

            flash.Append(mat.DOColor(Color.black, 2.0f * rate)).Play();
        }
    }

    protected virtual void OnDamage(float damage, float shield)
    {
        if (damage > 0)
        {
            DamageEffect(damage);
        }
    }

    public void Damage(float attack, Direction attackDir)
    {
        if (!IsAlive) return;

        float shield = Shield(attackDir);
        float damage = Mathf.Max(attack * ArmorMultiplier * GetDirMultiplier(attackDir) - shield, 0.0f);

        OnDamage(damage, shield);
        life -= damage;
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

    protected virtual void DamageEffect(float damage)
    {
        DamageFlash(damage);
    }

    public virtual void ResetStatus()
    {
        life = LifeMax;
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
