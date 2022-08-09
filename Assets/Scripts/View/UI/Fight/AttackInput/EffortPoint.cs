using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EffortPoint : TargetingUI
{
    [SerializeField] private RawImage pointImage = default;
    [SerializeField] private ParticleSystem chargingFX = default;

    private Tween rotateTween;

    protected override void Awake()
    {
        image = pointImage;
        rectTransform = GetComponent<RectTransform>();

        targetColor = DefaultColor();
        Enabled = isActive = isChangingColor = false;

        chargingFX?.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        rotateTween = pointImage.GetComponent<RectTransform>()
            .DORotate(new Vector3(0f, 0f, 90f), 1f, RotateMode.FastBeyond360)
            .SetRelative(true)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental)
            .AsReusable(gameObject);
    }

    protected override void OnActive(Vector2 pos)
    {
        rotateTween.Restart();
    }

    protected override void OnInactive()
    {
        chargingFX?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        rotateTween.Pause();
    }

    public override void EnableChargingUp()
    {
        chargingFX?.Play();
        SwitchColor(true);
    }

    public override void DisableChargingUp()
    {
        chargingFX?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        SwitchColor(false);
    }
}