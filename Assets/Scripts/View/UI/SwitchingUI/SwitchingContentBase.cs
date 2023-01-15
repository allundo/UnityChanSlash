using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;

public abstract class SwitchingContentBase : SwitchingUIBase
{
    [SerializeField] protected Button switchBtn = default;
    protected RectTransform btnRT;
    protected Vector2 currentBtnPos;

    public IObservable<Unit> Switch => switchBtn.OnClickAsObservable();

    protected override void Awake()
    {
        base.Awake();
        btnRT = switchBtn.GetComponent<RectTransform>();
    }

    void Start()
    {
        Switch.Subscribe(_ => HideUI()).AddTo(this);
    }
    protected override void SetPortraitPos()
    {
        base.SetPortraitPos();
        btnRT.anchoredPosition = currentBtnPos = new Vector2(-80f, 80f);
    }

    protected override void SetLandscapePos()
    {
        base.SetLandscapePos();
        btnRT.anchoredPosition = currentBtnPos = new Vector2(100f, -200f);
    }

    private void HideUI(float duration = 0.25f)
    {
        switchBtn.enabled = false;
        HideButton();
        SwitchUI(duration, () => SetEnable(false));
    }

    public void ShowUI(float duration = 0.25f, float delay = 0.25f)
    {
        SetEnable(true);
        ShowButton();
        DOVirtual.DelayedCall(delay, () => SwitchUI(duration, () => switchBtn.enabled = true)).Play();
    }

    public abstract void SetEnable(bool isEnabled);

    public void ShowButton() => switchBtn.gameObject.SetActive(true);
    public void HideButton() => switchBtn.gameObject.SetActive(false);
}
