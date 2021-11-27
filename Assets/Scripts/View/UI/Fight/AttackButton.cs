using UnityEngine;
using DG.Tweening;
using UniRx;

public class AttackButton : FadeEnable
{
    [SerializeField] float duration = 0.2f;
    protected UITween ui;

    private Tween expand;
    private Tween shrink;
    private Tween move;

    protected bool isFiring = false;

    public ISubject<Unit> AttackSubject { get; protected set; } = new Subject<Unit>();

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
        move = null;
    }

    public override Tween FadeOut(float duration = 0.2f, TweenCallback onPlay = null, TweenCallback onComplete = null, bool isContinuous = true)
    {
        onPlay = onPlay ?? (() => { });

        return base.FadeOut(duration,
            () =>
            {
                isFiring = false;
                onPlay();
            },
            onComplete,
            isContinuous);
    }

    public void Press(Vector2 pos)
    {
        move?.Kill();
        expand.Rewind();
        shrink.Rewind();
        FadeIn(0.1f, () => ui.SetScreenPos(pos), null, false).Play();
    }

    public void Release()
    {
        if (!isActive) return;

        isFiring = true;

        move = ui.MoveBackRatio(duration, 0.5f).Play();
        expand.Play();
        FadeOut(duration, null, null, false).Play();

        AttackSubject.OnNext(Unit.Default);
    }

    public void Cancel(float duration = 0.2f)
    {
        isFiring = true;

        move = ui.MoveBackRatio(duration, 0.25f).Play();
        shrink.Play();
        FadeOut(duration, null, null, false).Play();
    }
}
