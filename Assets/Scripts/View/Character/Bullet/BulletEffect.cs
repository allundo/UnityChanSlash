using UnityEngine;
using DG.Tweening;

public class BulletEffect : BodyEffect
{
    [SerializeField] protected ParticleSystem emitVfx = default;
    [SerializeField] protected ParticleSystem fireVfx = default;
    [SerializeField] protected ParticleSystem hitVfx = default;
    [SerializeField] protected ParticleSystem eraseVfx = default;
    [SerializeField] protected Transform meshTf;

    protected Vector3 defaultScale;
    protected Tween rolling;

    protected override void StoreMaterialColors()
    {
        foreach (Renderer renderer in meshTf.GetComponentsInChildren<Renderer>())
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

        defaultScale = meshTf.localScale;

        rolling = meshTf.DOLocalRotate(new Vector3(0f, 0f, 90f), 0.25f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetRelative()
            .SetLoops(-1, LoopType.Incremental);
    }

    public override void OnActive()
    {
        emitVfx?.Play();
        fireVfx?.Play();
        PlayFlash(FadeInTween(0.5f));
        rolling?.Restart();
    }

    public override void OnDie()
    {
        rolling?.Pause();
        emitVfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        eraseVfx?.Play();
    }

    public override void OnDamage(float damageRatio)
    {
        base.OnDamage(damageRatio);
        hitVfx?.Play();
    }

    public override void OnHeal(float healRatio) { }

    public override void OnLifeMax() { }

    protected override Tween GetFadeTween(bool isFadeIn, float duration = 0.5f)
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

        fade.Join(meshTf.DOScale(isFadeIn ? defaultScale : Vector3.zero, duration));

        if (isFadeIn)
        {
            Sequence blink = DOTween.Sequence();

            foreach (Material mat in flashMaterials)
            {
                blink.Join(mat.DOColor(Color.red, 0.2f).SetLoops(-1, LoopType.Yoyo));
            }

            fade.Append(blink);
        }

        return fade;
    }
}
