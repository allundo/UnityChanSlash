using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public interface IBodyEffect
{
    void OnActive();
    void OnDie();
    void OnMelt();
    void OnIced(Vector3 pos);
    void OnIceCrash(Vector3 pos);

    /// <summary>
    /// Play body effect on damage
    /// </summary>
    /// <param name="damageRatio">Normalized damage ratio to the life max</param>
    void OnDamage(float damageRatio, AttackType type, AttackAttr attr);

    /// <summary>
    /// Play body effect on heal
    /// </summary>
    /// <param name="healRatio">Normalized heal ratio to the life max</param>
    void OnHeal(float healRatio);

    Tween FadeInTween(float duration);
    Tween FadeOutTween(float duration);
    void KillAllTweens();
}

public class BodyEffect : MonoBehaviour, IBodyEffect
{
    [SerializeField] protected AudioSource dieSound = null;
    protected DamageSndData sndData;
    protected AnimationFX animFX;

    protected AudioSource SndDamage(AttackType type) => Util.Instantiate(sndData.Param((int)type).damage, transform);
    protected Dictionary<AttackType, AudioSource> damageSndSource = new Dictionary<AttackType, AudioSource>();
    protected void PlayDamage(AttackType type) => damageSndSource.LazyLoad(type, SndDamage).PlayEx();

    protected ParticleSystem LoadBodyVfx(VFXType type) => Util.Instantiate(GameManager.Instance.GetPrefabVFX(type));
    protected Dictionary<VFXType, ParticleSystem> crashVfxSource = new Dictionary<VFXType, ParticleSystem>();
    protected void PlayBodyVFX(VFXType type, Vector3 pos)
    {
        var vfx = crashVfxSource.LazyLoad(type, LoadBodyVfx);
        if (vfx == null) return;

        vfx.transform.position = pos;
        vfx.Play();
    }
    protected void StopBodyVFX(VFXType type) => crashVfxSource.LazyLoad(type, LoadBodyVfx)?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    protected virtual void StopAnimFX() { }

    protected List<Material> flashMaterials = new List<Material>();
    protected Tween prevFlash;

    protected virtual void Awake()
    {
        StoreMaterialColors();
        sndData = Resources.Load<DamageSndData>("DataAssets/Sound/DamageSndData");
        animFX = GetComponent<AnimationFX>();
    }

    protected void PlayFlash(Tween flash)
    {
        prevFlash?.Complete();
        prevFlash = flash.Play();
    }

    protected void PlayFlash(Color color, float duration)
    {
        Sequence flash = DOTween.Sequence();
        flashMaterials.ForEach(mat => flash.Join(mat.DOColor(color, duration)));
        PlayFlash(flash);
    }

    public virtual void OnActive()
    {
        PlayFlash(FadeInTween());
    }

    public virtual void OnDie()
    {
        dieSound.PlayEx();
        StopBodyVFX(VFXType.Iced);
        StopAnimFX();
    }

    public virtual void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        DamageSound(damageRatio, type);
        if (attr != AttackAttr.Ice) DamageFlash(damageRatio);
    }

    public virtual void OnHeal(float healRatio) { }

    protected virtual void StoreMaterialColors()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.HasProperty("_AdditiveColor"))
                {
                    mat.color = new Color(0, 0, 0, 1);
                    flashMaterials.Add(mat);
                }
            }
        }
    }

    protected virtual void DamageSound(float damageRatio, AttackType type) => PlayDamage(type);

    protected virtual void DamageFlash(float damageRatio)
    {
        if (damageRatio < 0.000001f) return;

        Sequence flash = DOTween.Sequence();

        foreach (Material mat in flashMaterials)
        {
            Sequence flashSub = DOTween.Sequence().Append(mat.DOColor(Color.white, 0.02f));

            if (damageRatio > 0.1f)
            {
                flashSub.Append(mat.DOColor(Color.black, 0.02f));
                flashSub.Append(mat.DOColor(Color.red, 0.02f));
            }

            flash.Join(flashSub.Append(mat.DOColor(Color.black, 2.0f * damageRatio)));
        }

        PlayFlash(flash);
    }

    public virtual void OnIced(Vector3 pos)
    {
        PlayBodyVFX(VFXType.Iced, pos);
        PlayFlash(new Color(0f, 0.5f, 0.5f, 1f), 0.1f);
    }
    public virtual void OnMelt()
    {
        StopBodyVFX(VFXType.Iced);
        PlayFlash(Color.black, 0.5f);
    }

    public virtual void OnIceCrash(Vector3 pos) { }

    public virtual Tween FadeInTween(float duration = 0.5f) => GetFadeTween(true, duration);
    public virtual Tween FadeOutTween(float duration = 0.5f) => GetFadeTween(false, duration);

    protected virtual Sequence GetFadeTween(bool isFadeIn, float duration = 0.5f) => GetFadeTween(isFadeIn ? 1f : 0f, duration);

    protected virtual Sequence GetFadeTween(float endValue, float duration = 0.5f)
    {
        Sequence fade = DOTween.Sequence();

        foreach (Material mat in flashMaterials)
        {
            fade.Join(
                DOTween.ToAlpha(
                    () => mat.color,
                    color => mat.color = color,
                    endValue,
                    duration
                )
                .SetEase(Ease.InSine)
            );
        }

        return fade;
    }

    public virtual void KillAllTweens()
    {
        prevFlash?.Kill();
    }
}
