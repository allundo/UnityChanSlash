using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class MobStatus : MonoBehaviour
{
    protected MobCommander commander = default;
    [SerializeField] private AudioSource dieSound = default;
    protected List<Material> flashMaterials = new List<Material>();

    public float Life { get; protected set; } = 0.0f;

    protected bool IsAlive => Life > 0.0f;

    [SerializeField] public float LifeMax = 10;

    public virtual int Attack { get; protected set; } = 1;

    protected virtual void Start()
    {
        commander = GetComponent<MobCommander>();

        StoreMaterialColors();
        ResetStatus();
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

    protected virtual void OnDie()
    {
        dieSound?.Play();

        commander.SetDie();
    }

    protected virtual void OnDamage() { }

    public void Damage(int damage)
    {
        if (!IsAlive) return;

        Life -= damage;
        DamageEffect(damage);

        if (IsAlive) return;

        OnDie();
    }

    protected virtual void DamageEffect(int damage)
    {
        DamageFlash(damage);
    }

    public virtual void ResetStatus()
    {
        Life = LifeMax;
    }
}