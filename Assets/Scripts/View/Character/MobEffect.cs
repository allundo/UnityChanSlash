using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class MobEffect : MonoBehaviour
{
    [SerializeField] private AudioSource dieSound = null;
    [SerializeField] private AudioSource damageSound = null;
    [SerializeField] private AudioSource criticalSound = null;

    protected List<Material> flashMaterials = new List<Material>();

    protected virtual void Awake()
    {
        StoreMaterialColors();
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

    protected void DamageFlash(float damageRatio)
    {
        if (damageRatio < 0.000001f) return;

        foreach (Material mat in flashMaterials)
        {
            Sequence flash = DOTween.Sequence().Append(mat.DOColor(Color.white, 0.02f));

            if (damageRatio > 0.1f)
            {
                flash.Append(mat.DOColor(Color.black, 0.02f));
                flash.Append(mat.DOColor(Color.red, 0.02f));
            }

            flash.Append(mat.DOColor(Color.black, 2.0f * damageRatio)).Play();
        }
    }

    protected void DamageSound(float damageRatio)
    {
        if (damageRatio < 0.000001f) return;

        Play(damageRatio > 0.1f ? criticalSound : damageSound);
    }

    public Tween FadeInTween(float duration = 0.5f) => GetFadeTween(true, duration);
    public Tween FadeOutTween(float duration = 0.5f) => GetFadeTween(false, duration);
    protected Tween GetFadeTween(bool isFadeIn, float duration = 0.5f)
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

        return fade;
    }
}
