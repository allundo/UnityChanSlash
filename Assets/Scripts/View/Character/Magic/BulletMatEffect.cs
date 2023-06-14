using UnityEngine;
using DG.Tweening;

public class BulletMatEffect : MatColorEffect
{
    protected Transform meshTf;
    protected Vector3 defaultScale;

    protected float dyingFXDuration;

    protected Tween rollingTween = null;
    protected Tween blinkTween = null;

    protected override void InitProperty(Material mat, int propID) => mat.color = new Color(0, 0, 0, 1);

    public BulletMatEffect(Transform targetTf, float dyingFXDuration = 0f, float cycleTimeSec = 0f, Color blinkColor = default) : base(targetTf)
    {
        meshTf = targetTf;
        defaultScale = meshTf.localScale;
        this.dyingFXDuration = dyingFXDuration;

        if (cycleTimeSec > 0f)
        {
            rollingTween = meshTf.DOLocalRotate(new Vector3(0f, 0f, 90f), cycleTimeSec * 0.25f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetRelative()
                .SetLoops(-1, LoopType.Incremental);
        }

        if (blinkColor != default)
        {
            Sequence blink = DOTween.Sequence();
            materials.ForEach(mat => blink.Join(mat.DOColor(blinkColor, 0.1f)));

            blinkTween = blink.SetLoops(-1, LoopType.Yoyo).AsReusable(meshTf.gameObject);
        }
    }

    public override void Activate(float duration = 0.25f)
    {
        base.Activate(duration);
        rollingTween?.Restart();
        blinkTween?.Restart();
    }

    public override void Inactivate(TweenCallback onComplete = null, float duration = 0.5f)
    {
        rollingTween?.Pause();
        blinkTween?.Pause();
        t.Complete();

        var seq = DOTween.Sequence().Append(FadeOutTween(duration));

        float extraDuration = dyingFXDuration - duration;
        if (extraDuration > 0) seq.AppendInterval(extraDuration);

        disappearTween = seq.OnComplete(onComplete).Play();
    }

    protected override Tween PlayExclusive(Tween matTween)
    {
        blinkTween?.Rewind();
        return t.PlayExclusive(matTween);
    }

    protected override Tween GetFadeTween(bool isFadeIn, float duration = 0.5f)
    {
        return meshTf.DOScale(isFadeIn ? defaultScale : Vector3.zero, duration);
    }

    public override void KillAllTweens()
    {
        rollingTween?.Kill();
        blinkTween?.Kill();
        base.KillAllTweens();
    }
}