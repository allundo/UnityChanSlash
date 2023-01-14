using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;

public abstract class SwitchingContentBase : SwitchingUIBase
{
    [SerializeField] protected Button switchBtn = default;

    public IObservable<Unit> Switch => switchBtn.OnClickAsObservable();

    void Start()
    {
        Switch.Subscribe(_ => HideUI()).AddTo(this);
    }

    private void HideUI(float duration = 0.25f)
    {
        switchBtn.enabled = false;
        HideButton();
        SwitchUI(duration, DisableUI);
    }

    public void ShowUI(float duration = 0.25f, float delay = 0.25f)
    {
        EnableUI();
        ShowButton();
        DOVirtual.DelayedCall(delay, () => SwitchUI(duration, () => switchBtn.enabled = true)).Play();
    }

    protected abstract void EnableUI();
    protected abstract void DisableUI();

    public void ShowButton() => switchBtn.gameObject.SetActive(true);
    public void HideButton() => switchBtn.gameObject.SetActive(false);
}
