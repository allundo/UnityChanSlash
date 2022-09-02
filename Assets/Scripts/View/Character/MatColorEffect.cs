using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public interface IMatColorEffect : IMatTransparentEffect
{
    void Flash(Color color, float duration);
    void DamageFlash(float damageRatio);
    void Activate(float duration);
    void Inactivate(TweenCallback onComplete, float duration);
}

public class MatColorEffect : MatTransparentEffect, IMatColorEffect
{
    protected Tween disappearTween;

    protected override string propName => "_AdditiveColor";
    protected override void InitProperty(Material mat, int propID) => mat.color = new Color(0, 0, 0, 0);

    public MatColorEffect(Transform targetTf) : base(targetTf) { }
    public MatColorEffect(List<Material> materials) : base(materials) { }

    public void Flash(Color color, float duration)
    {
        Sequence flash = DOTween.Sequence();
        materials.ForEach(mat => flash.Join(mat.DOColor(color, duration)));
        PlayExclusive(flash);
    }

    public virtual void DamageFlash(float damageRatio)
    {
        if (Mathf.Abs(damageRatio) < 0.000001f) return;

        bool isAbsorb = damageRatio < 0f;

        Sequence flash = DOTween.Sequence();

        foreach (Material mat in materials)
        {
            Sequence flashSub = DOTween.Sequence()
                .Append(mat.DOColor(Color.white, isAbsorb ? 0.5f : 0.04f));

            if (damageRatio > 0.1f)
            {
                flashSub.Append(mat.DOColor(Color.black, 0.04f));
                flashSub.Append(mat.DOColor(Color.red, 0.04f));
            }

            flash.Join(flashSub.Append(mat.DOColor(Color.black, isAbsorb ? 0.5f : 2.0f * damageRatio)));
        }

        PlayExclusive(flash);
    }

    public virtual void Activate(float duration = 0.5f)
    {
        InitEffects();
        FadeIn(duration);
    }

    public virtual void Inactivate(TweenCallback onComplete = null, float duration = 0.5f)
    {
        t.Complete();
        disappearTween = FadeOutTween(duration).OnComplete(onComplete).Play();
    }

    public override void KillAllTweens()
    {
        disappearTween?.Complete();
        t.Kill();
    }
}
