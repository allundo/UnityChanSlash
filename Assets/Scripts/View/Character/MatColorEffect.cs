using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public interface IMatColorEffect : IMaterialEffect
{
    void PlayExclusive(Tween matTween);
    void Flash(Color color, float duration);
    void DamageFlash(float damageRatio);
    void FadeIn(float duration);
    void FadeOut(float duration);
    void Activate(float duration);
    void Inactivate(TweenCallback onComplete, float duration);
}

public class MatColorEffect : MaterialEffect, IMatColorEffect
{
    protected Tween prevTween;
    protected Sequence disappearSeq;

    protected override string propName => "_AdditiveColor";
    protected override void InitProperty(Material mat, int propID) => mat.color = new Color(0, 0, 0, 1);
    public MatColorEffect(Transform targetTf) : base(targetTf) { }
    public MatColorEffect(List<Material> materials) : base(materials) { }

    public virtual void PlayExclusive(Tween matTween)
    {
        prevTween?.Complete(true);
        prevTween = matTween.Play();
    }

    public void Flash(Color color, float duration)
    {
        Sequence flash = DOTween.Sequence();
        materials.ForEach(mat => flash.Join(mat.DOColor(color, duration)));
        PlayExclusive(flash);
    }

    public virtual void DamageFlash(float damageRatio)
    {
        if (damageRatio < 0.000001f) return;

        Sequence flash = DOTween.Sequence();

        foreach (Material mat in materials)
        {
            Sequence flashSub = DOTween.Sequence().Append(mat.DOColor(Color.white, 0.02f));

            if (damageRatio > 0.1f)
            {
                flashSub.Append(mat.DOColor(Color.black, 0.02f));
                flashSub.Append(mat.DOColor(Color.red, 0.02f));
            }

            flash.Join(flashSub.Append(mat.DOColor(Color.black, 2.0f * damageRatio)));
        }

        PlayExclusive(flash);
    }

    protected virtual Tween FadeInTween(float duration = 0.5f) => GetFadeTween(true, duration);
    protected virtual Tween FadeOutTween(float duration = 0.5f) => GetFadeTween(false, duration);

    protected virtual Tween GetFadeTween(bool isFadeIn, float duration = 0.5f) => GetFadeTween(isFadeIn ? 1f : 0f, duration);

    protected virtual Tween GetFadeTween(float endValue, float duration = 0.5f)
    {
        Sequence fade = DOTween.Sequence();

        foreach (Material mat in materials)
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

    public virtual void FadeIn(float duration = 0.5f)
    {
        PlayExclusive(FadeInTween(duration));
    }

    public virtual void FadeOut(float duration = 0.5f)
    {
        PlayExclusive(FadeOutTween(duration));
    }

    public virtual void Activate(float duration = 0.5f)
    {
        InitEffects();
        FadeIn(duration);
    }

    public virtual void Inactivate(TweenCallback onComplete = null, float duration = 0.5f)
    {
        prevTween?.Complete(true);

        disappearSeq = DOTween.Sequence()
            .Append(FadeOutTween(duration))
            .AppendCallback(onComplete);

        disappearSeq.Play();
    }

    public override void KillAllTweens()
    {
        disappearSeq?.Complete(true);
        prevTween?.Kill();
    }
}