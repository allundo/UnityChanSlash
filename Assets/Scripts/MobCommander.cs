using UnityEngine;
using DG.Tweening;
using UniRx;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(MobAnimator))]
[RequireComponent(typeof(MapUtil))]
public abstract class MobCommander : MonoBehaviour
{
    public MobAnimator anim { get; protected set; }
    protected MobStatus status;

    protected bool isCommandValid
    {
        get
        {
            return IsCommandValid.Value;
        }
        set
        {
            IsCommandValid.Value = value;
        }
    }
    protected IReactiveProperty<bool> IsCommandValid;
    protected ReactiveCommand<Command> InputReactive;

    public bool IsIdling => currentCommand == null;

    protected Queue<Command> cmdQueue = new Queue<Command>();
    public Command currentCommand { get; protected set; } = null;
    protected Command die = null;

    public MapUtil map { get; protected set; } = default;

    protected virtual void Awake()
    {
        anim = GetComponent<MobAnimator>();
        map = GetComponent<MapUtil>();

        IsCommandValid = new ReactiveProperty<bool>(true);
        InputReactive = new ReactiveCommand<Command>(IsCommandValid);
    }

    protected virtual void Start()
    {
        status = GetComponent<MobStatus>();

        SetCommands();

        InputReactive.Subscribe(com => EnqueueCommand(com, IsIdling));
    }

    /// <summary>
    /// This method is called by Start(). Override it to customize commands' behavior.
    /// </summary>
    protected virtual void SetCommands()
    {
        die = new DieCommand(this, 0.1f);
    }

    protected virtual void Update()
    {
        InputReactive.Execute(GetCommand());
    }

    protected virtual void EnqueueCommand(Command cmd, bool dispatch = false)
    {
        if (cmd == null) return;

        cmdQueue.Enqueue(cmd);
        isCommandValid = false;

        if (dispatch)
        {
            DispatchCommand();
        }
    }

    protected virtual bool DispatchCommand()
    {
        if (cmdQueue.Count > 0)
        {
            currentCommand = cmdQueue.Dequeue();
            currentCommand.Execute();
            return true;
        }

        currentCommand = null;
        return false;
    }

    protected abstract Command GetCommand();

    public virtual void SetDie()
    {
        map.ResetOnCharactor();

        cmdQueue.Clear();
        currentCommand?.Cancel();
        EnqueueDie();
    }

    protected virtual void EnqueueDie()
    {
        EnqueueCommand(die, true);
    }

    public virtual void Respawn()
    {
        status.ResetStatus();

        transform.gameObject.SetActive(true);
        isCommandValid = true;

        map.SetPosition();

        // TODO: Fade-in with custom shader
    }

    protected virtual void Destory()
    {
        currentCommand = null;

        transform.gameObject.SetActive(false);

        // TODO: Fade-out with custom shader
    }

    public abstract class Command
    {
        public static float DURATION_UNIT = 0.6f;
        protected const float TILE_UNIT = 2.5f;

        protected float duration;

        protected MobCommander commander;
        protected TweenMove tweenMove;

        public Command(MobCommander commander, float duration)
        {
            this.duration = duration * DURATION_UNIT;
            this.commander = commander;
            this.tweenMove = new TweenMove(commander.transform, this.duration);
        }

        protected Tween playingTween = null;
        protected Tween validateTween = null;
        public virtual void Cancel()
        {
            playingTween?.Kill();
            validateTween?.Kill();
        }

        public abstract void Execute();
        public virtual float Speed => 0.0f;
        public virtual float RSpeed => 0.0f;

        protected Sequence JoinTweens(params Tween[] tweens)
        {
            Sequence seq = DOTween.Sequence();

            for (int i = 0; i < tweens.Length; i++)
            {
                seq.Join(tweens[i]);
            }

            return seq;
        }

        /// <summary>
        /// Play tween and next command dispatching on complete
        /// </summary>
        /// <param name="tween">Tween (or Sequence) to play</param>
        /// <param name="OnComplete">Additional process on complete</param>
        protected void PlayTween(Tween tween, Action OnComplete = null)
        {
            playingTween = tween;

            tween.OnComplete(DispatchFinally(OnComplete)).Play();
        }

        /// <summary>
        /// Reserve validating of next command input
        /// </summary>
        /// <param name="timing">Validate timing specified by normalized (0.0f,1.0f) command duration</param>
        protected void SetValidateTimer(float timing = 0.5f)
        {
            validateTween = tweenMove.SetDelayedCall(timing, () => { commander.isCommandValid = true; });
        }

        /// <summary>
        /// Reserve next command dispatching after command duration.
        /// </summary>
        /// <param name="OnComplete">Additional process on complete</param>
        protected void SetDispatchFinal(Action OnComplete = null)
        {
            tweenMove.SetFinallyCall(DispatchFinally(OnComplete));
        }

        /// <summary>
        /// Reserve destory commander object after command duration
        /// </summary>
        protected void SetDestoryFinal()
        {
            tweenMove.SetFinallyCall(() => commander.Destory());
        }

        private TweenCallback DispatchFinally(Action OnComplete = null)
        {
            return () =>
            {
                if (OnComplete != null) OnComplete();
                commander.DispatchCommand();
            };
        }
    }

    public class DieCommand : Command
    {
        public DieCommand(MobCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            commander.anim.die.Fire();
            SetDestoryFinal();
        }
    }
}
