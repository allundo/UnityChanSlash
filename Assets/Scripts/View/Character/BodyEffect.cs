using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public interface IBodyEffect
{
    void OnActive();
    void OnDie();

    /// <summary>
    /// Play body effect on damage
    /// </summary>
    /// <param name="damageRatio">Normalized damage ratio to the life max</param>
    void OnDamage(float damageRatio, AttackType type);

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
    [SerializeField] protected DamageSndData data;

    protected AudioSource SndDamage(AttackType type) => Util.Instantiate(data.Param((int)type).damage, transform);
    protected Dictionary<AttackType, AudioSource> damageSndSource = new Dictionary<AttackType, AudioSource>();
    protected void PlayDamage(AttackType type) => damageSndSource.LazyLoad(type, SndDamage).PlayEx();

    protected List<Material> flashMaterials = new List<Material>();
    protected Tween prevFlash;

    protected virtual void Awake()
    {
        StoreMaterialColors();
    }

    protected void PlayFlash(Tween flash)
    {
        prevFlash?.Complete();
        prevFlash = flash.Play();
    }

    public virtual void OnActive()
    {
        PlayFlash(FadeInTween());
    }

    public virtual void OnDie()
    {
        dieSound.PlayEx();
    }

    public virtual void OnDamage(float damageRatio, AttackType type = AttackType.None)
    {
        DamageSound(damageRatio, type);
        DamageFlash(damageRatio);
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
