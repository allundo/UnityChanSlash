using UnityEngine;
using DG.Tweening;
using UniRx;
using System;

public class AttackButton : FadeUI
{
    [SerializeField] private int frames = 60;
    [SerializeField] private float motionFrameRate = 30f;
    [SerializeField] private float cancelStart = 1f;
    [SerializeField] private float coolTimeRatio = 1f;

    public static readonly int FRAME_RATE = Constants.FRAME_RATE;
    public static readonly float FRAME_UNIT = Constants.FRAME_SEC_UNIT;
    public static readonly float ATTACK_SPEED = Constants.PLAYER_ATTACK_SPEED;
    public static readonly float CRITICAL_SPEED = Constants.PLAYER_CRITICAL_SPEED;

    private float motionFrames;
    private float duration;

    private IFadeUI region;
    public void SetRegion(IFadeUI region)
    {
        this.region = region;
    }

    protected UITween ui;

    private Tween expand;
    private Tween shrink;
    private Tween move = null;

    private Tween coolTimer = null;
    private float countTime = 0f;

    /// <summary>
    /// Player can reserve pressing AttackButton during its on cool time. <br />
    /// This flag allows player to reserve next attack by pressing AttackButtonRegion on cool time.
    /// </summary>
    protected bool isPressEnable = false;

    /// <summary>
    /// True if reserving an AttackButton.
    /// </summary>
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
        motionFrames = (float)frames / motionFrameRate / ATTACK_SPEED * (float)FRAME_RATE;
        duration = (float)frames / motionFrameRate / CRITICAL_SPEED;

        ui = new UITween(gameObject);
        FadeInit();
    }

    protected virtual void Start()
    {
        expand = ui.Resize(1.5f, duration, true).OnComplete(() => ui.ResetSize());
        shrink = ui.Resize(0.5f, 0.2f, true).OnComplete(() => ui.ResetSize());

        EnableButton();
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
        ui.SetScreenPos(pos);
        FadeActivate(0.1f);

        DisableButton();
    }

    public void Release()
    {
        isPressReserved = false;

        if (!isActive) return;

        move = ui.MoveBackRatio(duration, 0.5f).Play();
        expand.Play();

        FadeInactivate(duration);
        coolTimer = StartCoolTime(CoolTime);

        attackSubject.OnNext(this);
    }

    public void Cancel(float duration = 0.2f)
    {
        move = ui.MoveBackRatio(duration, 0.25f).Play();
        shrink.Play();

        FadeInactivate(duration);

        EnableButton();
    }

    protected override void BeforeFadeOut()
    {
        isPressReserved = false;
    }

    private void DisableButton()
    {
        isPressEnable = true;
        isPressReserved = false;
        region.FadeInactivate(0.1f);
    }

    private void EnableButton()
    {
        isPressEnable = false;
        if (isPressReserved)
        {
            StartPressing(pressPos);
        }
        region.FadeActivate(0.2f);
    }

    public void SetCoolTime(float coolTime)
    {
        if (countTime > coolTime) return;

        DisableButton();
        coolTimer?.Kill();
        coolTimer = StartCoolTime(coolTime);
    }

    private Tween StartCoolTime(float coolTime)
    {
        countTime = coolTime;
        return DOTween.To(() => countTime, time => countTime = time, 0f, coolTime).OnComplete(EnableButton).Play();
    }

    public void Inactivate()
    {
        coolTimer?.Kill();
        countTime = 0f;
        isPressEnable = false;
        FadeInactivate(0.1f);
    }

    public override void KillTweens()
    {
        fade.KillTweens();
        region.KillTweens();
        expand?.Kill();
        shrink?.Kill();
        move?.Kill();
        coolTimer?.Kill();
    }
}
