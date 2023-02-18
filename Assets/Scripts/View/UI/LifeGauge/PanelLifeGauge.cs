using UnityEngine;
using DG.Tweening;

public class PanelLifeGauge : FadeUI
{
    [SerializeField] private PanelGauge greenGauge = default;
    [SerializeField] private PanelGauge redGauge = default;

    private Tween visualTimer = null;
    private Canvas canvas;
    private float angleX;

    void Start()
    {
        canvas = transform.parent.GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        visualTimer = DOVirtual.DelayedCall(3f, () => FadeInactivate()).AsReusable(gameObject);

        canvas.transform.rotation = canvas.worldCamera.transform.rotation * Quaternion.Euler(0, 180, 0);
        angleX = canvas.transform.localEulerAngles.x;
    }

    public void UpdateLife(float life, float lifeMax) => UpdateGauge(life / lifeMax);

    public void UpdateGauge(float lifeRatio)
    {
        canvas.transform.rotation = Quaternion.Euler(angleX, PlayerInfo.Instance.Dir.Backward.Angle.y, 0f);
        greenGauge.UpdateGauge(lifeRatio);
        redGauge.UpdateGauge(lifeRatio);
    }

    protected override void OnFadeEnable(float fadeDuration)
    {
        visualTimer?.Rewind();
        visualTimer?.Restart();

        greenGauge.FadeActivate(fadeDuration);
        redGauge.FadeActivate(fadeDuration);
    }

    protected override void BeforeFadeOut()
    {
        greenGauge.FadeInactivate();
        redGauge.FadeInactivate();
    }

    protected override void OnDisable()
    {
        visualTimer?.Rewind();

        greenGauge.Disable();
        redGauge.Disable();
    }
}
