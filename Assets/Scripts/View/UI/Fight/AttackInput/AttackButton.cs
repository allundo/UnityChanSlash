using UnityEngine;
using DG.Tweening;
using UniRx;
using System;

public class AttackButton : FadeEnable
{
    [SerializeField] private float duration = 0.2f;

    private FadeEnable region;
    public void SetRegion(FadeEnable region)
    {
        this.region = region;
        region.Activate();
    }

    protected UITween ui;

    private Tween expand;
    private Tween shrink;
    private Tween move = null;

    private Tween coolTimer = null;
    private float countTime = 0f;

    protected bool isFiring = false;

    private ISubject<Unit> attackSubject = new Subject<Unit>();
    public IObservable<Unit> ObservableAtk => attackSubject;

    public float CoolTime => duration * 1.2f;
    public float CancelTime => duration * 0.4f;

    protected override void Awake()
    {
        fade = new FadeTween(gameObject);
        ui = new UITween(gameObject);

        Inactivate();
    }

    protected virtual void Start()
    {
        expand = ui.Resize(1.5f, duration, true).OnComplete(() => ui.ResetSize());
        shrink = ui.Resize(0.5f, 0.2f, true).OnComplete(() => ui.ResetSize());
    }

    public void Press(Vector2 pos)
    {
        if (isFiring) return;

        move?.Kill();
        expand.Rewind();
        shrink.Rewind();
        FadeIn(0.1f, () => ui.SetScreenPos(pos), null, false).Play();

        Disable();
    }

    public void Release()
    {
        if (!isActive) return;

        move = ui.MoveBackRatio(duration, 0.5f).Play();
        expand.Play();
        FadeOut(duration, null, null, false).Play();

        StartCoolTime(CoolTime);

        attackSubject.OnNext(Unit.Default);
    }

    public void Cancel(float duration = 0.2f)
    {
        isFiring = true;

        move = ui.MoveBackRatio(duration, 0.25f).Play();
        shrink.Play();
        FadeOut(duration, null, null, false).Play();

        Enable();
    }

    private void Disable()
    {
        isFiring = true;
        region.FadeOut(0.1f).Play();
    }

    private void Enable()
    {
        isFiring = false;
        region.FadeIn(0.2f).Play();
    }

    public void SetCoolTime(float coolTime)
    {
        if (countTime > coolTime) return;

        Disable();
        coolTimer?.Kill();
        coolTimer = StartCoolTime(coolTime);
    }

    private Tween StartCoolTime(float coolTime)
    {
        countTime = coolTime;
        return DOTween.To(() => countTime, time => countTime = time, 0f, coolTime).OnComplete(Enable).Play();
    }
}
