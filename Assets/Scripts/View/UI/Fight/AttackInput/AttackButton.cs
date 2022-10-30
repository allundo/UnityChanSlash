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

    protected bool isPressEnable = false;
    public bool isPressReserved { get; protected set; } = false;
    public Vector2 pressPos { get; private set; } = Vector2.zero;

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

        Enable();
    }

    public void Press(Vector2 pos)
    {
        if (isPressEnable)
        {
            isPressReserved = true;
            pressPos = pos;
            return;
        }

        StartPressing(pos);
    }

    protected void StartPressing(Vector2 pos)
    {
        move?.Kill();
        expand.Rewind();
        shrink.Rewind();
        FadeIn(0.1f, () => ui.SetScreenPos(pos), null, false).Play();

        Disable();
    }

    public void Release()
    {
        isPressReserved = false;

        if (!isActive) return;

        move = ui.MoveBackRatio(duration, 0.5f).Play();
        expand.Play();
        FadeOut(duration, null, null, false).Play();

        StartCoolTime(CoolTime);

        attackSubject.OnNext(this);
    }

    public void Cancel(float duration = 0.2f)
    {
        isPressReserved = false;

        move = ui.MoveBackRatio(duration, 0.25f).Play();
        shrink.Play();
        FadeOut(duration, null, null, false).Play();

        Enable();
    }

    protected override void OnFadeOut()
    {
        isPressReserved = false;
    }

    private void Disable()
    {
        isPressEnable = true;
        isPressReserved = false;
        region.FadeOut(0.1f).Play();
    }

    private void Enable()
    {
        isPressEnable = false;
        if (isPressReserved)
        {
            StartPressing(pressPos);
        }
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
