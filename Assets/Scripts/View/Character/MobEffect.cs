using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public interface IBodyEffect
{
    void OnDie();

    /// <summary>
    /// Play body effect on damage
    /// </summary>
    /// <param name="damageRatio">Normalized damage ratio to the life max</param>
    void OnDamage(float damageRatio);

    /// <summary>
    /// Play body effect on heal
    /// </summary>
    /// <param name="healRatio">Normalized heal ratio to the life max</param>
    void OnHeal(float healRatio);

    void OnLifeMax();

    Tween FadeInTween(float duration);
    Tween FadeOutTween(float duration);
}

public class MobEffect : MonoBehaviour, IBodyEffect
{
    [SerializeField] private AudioSource dieSound = null;
    [SerializeField] private AudioSource damageSound = null;
    [SerializeField] private AudioSource criticalSound = null;
    [SerializeField] private AudioSource lifeMaxSound = null;

    protected List<Material> flashMaterials = new List<Material>();
    protected Tween prevFlash;

    protected virtual void Awake()
    {
        StoreMaterialColors();
    }

    protected void PlayFlash(Tween flash)
    {
        prevFlash?.Kill();
        prevFlash = flash.Play();
    }

    protected void Play(AudioSource src)
    {
        if (src != null)
        {
            src.Play();
        }
    }

    public virtual void OnDie()
    {
        Play(dieSound);
    }

    public virtual void OnDamage(float damageRatio)
    {
        DamageSound(damageRatio);
        DamageFlash(damageRatio);
    }

    public virtual void OnHeal(float healRatio)
    {
        HealFlash(healRatio * 0.5f);
    }

    public virtual void OnLifeMax()
    {
        Play(lifeMaxSound);
    }

    protected void StoreMaterialColors()
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

    protected void DamageFlash(float damageRatio)
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

    protected void HealFlash(float duration)
    {
        Sequence flash = DOTween.Sequence();

        foreach (Material mat in flashMaterials)
        {
            flash.Join(
                DOTween.Sequence()
                    .Append(mat.DOColor(new Color(0.5f, 0.5f, 1f), duration * 0.5f))
                    .Append(mat.DOColor(Color.black, duration * 0.5f).SetEase(Ease.InQuad))
            );
        }

        PlayFlash(flash);
    }

    protected void DamageSound(float damageRatio)
    {
        if (damageRatio < 0.000001f) return;

        Play(damageRatio > 0.1f ? criticalSound : damageSound);
    }

    public Tween FadeInTween(float duration = 0.5f) => GetFadeTween(true, duration);
    public Tween FadeOutTween(float duration = 0.5f) => GetFadeTween(false, duration);

    protected virtual Tween GetFadeTween(bool isFadeIn, float duration = 0.5f)
    {
        Sequence fade = DOTween.Sequence();

        foreach (Material mat in flashMaterials)
        {
            fade.Join(
                DOTween.ToAlpha(
                    () => mat.color,
                    color => mat.color = color,
                    isFadeIn ? 1.0f : 0.0f,
                    duration
                )
                .SetEase(Ease.InSine)
            );
        }

        return fade.OnPlay(() => prevFlash?.Kill());
    }
}
