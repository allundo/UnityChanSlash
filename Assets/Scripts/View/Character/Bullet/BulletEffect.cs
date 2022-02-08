using UnityEngine;
using DG.Tweening;

public class BulletEffect : BodyEffect
{
    [SerializeField] protected ParticleSystem emitVfx = default;
    [SerializeField] protected ParticleSystem fireVfx = default;
    [SerializeField] protected ParticleSystem hitVfx = default;
    [SerializeField] protected ParticleSystem eraseVfx = default;

    [SerializeField] protected AudioSource fireSound = default;

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
        fireSound.PlayEx();
        emitVfx?.Play();
        fireVfx?.Play();
        PlayFlash(FadeInTween(0.25f));
        rolling?.Restart();
    }

    public override void OnDie()
    {
        rolling?.Pause();
        eraseVfx?.Play();
        PlayFlash(FadeOutTween(0.5f));
        emitVfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public override void OnDamage(float damageRatio, AttackType type = AttackType.None)
    {
        DamageFlash(damageRatio);
        hitVfx?.Play();
    }

    public override void OnHeal(float healRatio) { }

    protected override Sequence GetFadeTween(bool isFadeIn, float duration = 0.5f)
    {
        Sequence fade = base.GetFadeTween(isFadeIn, duration);

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

    public override void KillAllTweens()
    {
        prevFlash?.Kill();
        rolling?.Kill();
    }
}
