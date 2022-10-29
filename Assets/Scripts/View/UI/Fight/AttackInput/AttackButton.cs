using UnityEngine;
using DG.Tweening;
using UniRx;
using System;

public class AttackButton : FadeEnable
{
    [SerializeField] private int frames = 60;
    [SerializeField] private float motionFrameRate = 30f;
    [SerializeField] private float cancelStart = 1f;
    [SerializeField] private float coolTimeRatio = 1f;

    public static readonly float FRAME_RATE = Constants.FRAME_RATE;
    public static readonly float FRAME_UNIT = Constants.FRAME_SEC_UNIT;
    public static readonly float ATTACK_SPEED = Constants.PLAYER_ATTACK_SPEED;
    public static readonly float CRITICAL_SPEED = Constants.PLAYER_CRITICAL_SPEED;

    private float motionFrames;
    private float duration;

    private FadeEnable region;
    public void SetRegion(FadeEnable region)
    {
        this.region = region;
    }

    protected UITween ui;

    private Tween expand;
    private Tween shrink;
    private Tween move = null;

    private Tween coolTimer = null;
    private float countTime = 0f;

    protected bool isFiring = false;

    private ISubject<AttackButton> attackSubject = new Subject<AttackButton>();
    public IObservable<AttackButton> ObservableAtk => attackSubject;

    public float CoolTime => motionFrames * FRAME_UNIT * coolTimeRatio;
    public float CancelTime => CoolTime * cancelStart * 0.5f;
    public float MotionFrames => motionFrames;
    public float CancelStart => cancelStart;

    protected override void Awake()
    {
        motionFrames = (float)frames / motionFrameRate / ATTACK_SPEED * FRAME_RATE;
        duration = (float)frames / motionFrameRate / CRITICAL_SPEED;

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

        attackSubject.OnNext(this);
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
