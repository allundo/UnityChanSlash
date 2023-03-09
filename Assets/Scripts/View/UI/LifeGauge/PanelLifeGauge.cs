using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;

public class PanelLifeGauge : FadeUI, ISpawnObject<PanelLifeGauge>
{
    [SerializeField] private Image gauge = default;
    [SerializeField] private PanelGauge greenBar = default;
    [SerializeField] private PanelGauge redBar = default;
    [SerializeField] private float offsetY = 1.7f;

    private Tween visualTimer = null;
    private Canvas canvas;
    private float angleX;
    private IEnemyStatus status;
    private IDisposable detectDir = null;
    private IDisposable lifeChange = null;

    public IObservable<IEnemyStatus> DetectDisable => disableSubject;
    private ISubject<IEnemyStatus> disableSubject = new Subject<IEnemyStatus>();

    protected override FadeTween FadeComponent() => new FadeTween(gauge, uiAlpha * maxAlpha);

    protected override void Awake()
    {
        base.Awake();

        canvas = GetComponent<Canvas>();
        visualTimer = DOVirtual.DelayedCall(3f, () => FadeInactivate()).AsReusable(gameObject);
    }

    public PanelLifeGauge OnSpawn(Vector3 pos, IDirection dir, float duration = 0.2f)
    {
        gameObject.SetActive(true);

        SetPos(pos);

        canvas.worldCamera = Camera.main;
        transform.rotation = canvas.worldCamera.transform.rotation * Quaternion.Euler(0, 180, 0);
        angleX = transform.localEulerAngles.x;

        var info = PlayerInfo.Instance;
        SetDir(info.Dir);

        detectDir = info.DirObservable
            .Where(_ => isActive)
            .Subscribe(dir => SetDir(dir))
            .AddTo(gameObject);

        return this;
    }

    public PanelLifeGauge SetStatus(IEnemyStatus status)
    {
        this.status = status;
        float lifeMax = status.LifeMax.Value;

        lifeChange = status.Life
            .SkipLatestValueOnSubscribe()
            .Subscribe(life => UpdateGauge(life / lifeMax))
            .AddTo(gameObject);

        return this;
    }

    public void Activate() => FadeActivate();
    public void Inactivate() => Disable();

    void Update()
    {
        if (status != null) SetPos(status.corePos);
    }

    private void SetPos(Vector3 corePos)
    {
        transform.position = new Vector3(corePos.x, corePos.y + offsetY, corePos.z);
    }

    private void SetDir(IDirection playerDir)
    {
        canvas.transform.rotation = Quaternion.Euler(angleX, playerDir.Backward.Angle.y, 0f);
    }

    private void UpdateGauge(float lifeRatio)
    {
        FadeActivate();
        greenBar.UpdateGauge(lifeRatio);
        redBar.UpdateGauge(lifeRatio);
    }

    protected override void OnFadeEnable(float fadeDuration)
    {
        visualTimer?.Rewind();
        visualTimer?.Restart();

        greenBar.FadeActivate(fadeDuration);
        redBar.FadeActivate(fadeDuration);
    }

    protected override void BeforeFadeOut()
    {
        greenBar.FadeInactivate();
        redBar.FadeInactivate();
    }

    protected override void OnDisable()
    {
        detectDir?.Dispose();
        lifeChange?.Dispose();

        visualTimer?.Rewind();

        greenBar.Disable();
        redBar.Disable();

        gameObject.SetActive(false);
        disableSubject.OnNext(status);
    }
}
