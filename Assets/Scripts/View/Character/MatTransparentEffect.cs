using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public interface IMatTransparentEffect : IMaterialEffect
{
    void FadeIn(float duration = 0.5f);
    void FadeOut(float duration = 0.5f);
}

public class MatTransparentEffect : MaterialEffect, IMatTransparentEffect
{
    protected override string propName => "_Color";
    protected override void InitProperty(Material mat, int propID) => mat.color = new Color(1, 1, 1, 1);

    protected TweenUtil t = new TweenUtil();
    protected virtual Tween PlayExclusive(Tween tween) => t.PlayExclusive(tween);

    public MatTransparentEffect(Transform targetTf) : base(targetTf) { }
    public MatTransparentEffect(List<Material> materials) : base(materials) { }

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

    public override void KillAllTweens()
    {
        t.Kill();
    }
}
