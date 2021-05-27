using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(MobAnimator))]
[RequireComponent(typeof(MapUtil))]
public abstract class MobCommander : MonoBehaviour
{
    public MobAnimator anim { get; protected set; }
    protected MobStatus status;

    protected bool isCommandValid = true;
    protected bool IsIdling => currentCommand == null;


    protected Queue<Command> cmdQueue = new Queue<Command>();
    public Command currentCommand { get; protected set; } = null;
    protected Command die = null;

    public MapUtil map { get; protected set; } = default;

    protected virtual void Awake()
    {
        anim = GetComponent<MobAnimator>();
        map = GetComponent<MapUtil>();
    }

    protected virtual void Start()
    {
        status = GetComponent<MobStatus>();

        SetPosition();
        SetCommands();
    }

    /// <summary>
    /// This method is called in Start(). By overriding it, you can change start position.
    /// </summary>
    /// <param name="tf"></param>
    protected virtual void SetPosition()
    {
        // TODO: Charactor position should be set by GameManager
        map.SetPosition();
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
        InputCommand();
        SetSpeed();
    }

    protected virtual void InputCommand()
    {
        if (!isCommandValid)
        {
            return;
        }

        Command cmd = GetCommand();

        if (cmd != null)
        {
            EnqueueCommand(cmd, IsIdling);
        }
    }

    protected virtual void EnqueueCommand(Command cmd, bool dispatch = false)
    {
        cmdQueue.Enqueue(cmd);
        isCommandValid = false;

        if (dispatch)
        {
            DispatchCommand();
        }
    }

    protected virtual void EnqueueDie()
    {
        EnqueueCommand(die, true);
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

    public virtual void SetSpeed()
    {
        anim.speed.Float = IsIdling ? 0.0f : currentCommand.Speed;
    }

    public virtual void Respawn()
    {
        status.ResetStatus();

        transform.gameObject.SetActive(true);
        isCommandValid = true;

        SetPosition();

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

        public Command(MobCommander commander, float duration)
        {
            this.duration = duration * DURATION_UNIT;
            this.commander = commander;
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

        protected Tween GetLinearMove(Vector3 moveVector)
        {
            return commander.transform.DOMove(moveVector, duration)
                .SetRelative()
                .SetEase(Ease.Linear);
        }

        protected Tween GetRotate(int angle = 90)
        {
            return commander.transform.DORotate(new Vector3(0, angle, 0), duration)
                .SetRelative()
                .SetEase(Ease.InCubic);
        }

        protected Sequence GetJumpSequence(Vector3 moveVector, float jumpPower = 1.0f, float edgeTime = 0.3f, float takeoffRate = 0.01f)
        {
            float middleTime = duration - 2 * edgeTime;
            float flyingRate = 1.0f - takeoffRate;

            return DOTween.Sequence()
                .Append(
                    commander.transform.DOMove(moveVector * takeoffRate, edgeTime).SetEase(Ease.OutExpo).SetRelative()
                )
                .Append(
                    commander.transform.DOJump(moveVector * flyingRate, jumpPower, 1, middleTime).SetRelative()
                )
                .AppendInterval(edgeTime);
        }

        /// <summary>
        /// Play tween and next command dispatching on complete
        /// </summary>
        /// <param name="move">Tween (or Sequence) to play</param>
        /// <param name="OnComplete">Additional process on complete</param>
        protected void PlayTweenMove(Tween move, Action OnComplete = null)
        {
            playingTween = move;

            move.OnComplete(DispatchFinally(OnComplete)).Play();
        }

        /// <summary>
        /// Reserve validating of next command input
        /// </summary>
        /// <param name="timing">Validate timing specified by normalized (0.0f,1.0f) command duration</param>
        protected void SetValidateTimer(float timing = 0.5f)
        {
            validateTween = SetDelayedCall(timing, () => { commander.isCommandValid = true; });
        }

        /// <summary>
        /// Reserve processing at specified timing
        /// </summary>
        /// <param name="timing">Process start timing specified by normalized (0.0f,1.0f) command duration</param>
        /// <param name="callback"></param>
        protected Tween SetDelayedCall(float timing, TweenCallback callback)
        {
            return DOVirtual.DelayedCall(duration * timing, callback);
        }

        /// <summary>
        /// Reserve next command dispatching after command duration.
        /// </summary>
        /// <param name="OnComplete">Additional process on complete</param>
        protected void SetDispatchFinal(Action OnComplete = null)
        {
            SetFinallyCall(DispatchFinally(OnComplete));
        }

        /// <summary>
        /// Reserve processing after command duration
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected Tween SetFinallyCall(TweenCallback callback)
        {
            return DOVirtual.DelayedCall(duration, callback);
        }

        /// <summary>
        /// Reserve destory commander object after command duration
        /// </summary>
        protected void SetDestoryFinal()
        {
            SetFinallyCall(() => commander.Destory());
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

    public abstract class ActionCommand : Command
    {
        protected MobAnimator.AnimatorTrigger trigger;
        public ActionCommand(MobCommander commander, float duration, MobAnimator.AnimatorTrigger trigger) : base(commander, duration)
        {
            this.trigger = trigger;
        }

        public override void Execute()
        {
            trigger.Fire();

            SetValidateTimer();
            SetDispatchFinal();
        }
    }
    public class DieCommand : ActionCommand
    {
        public DieCommand(MobCommander commander, float duration) : base(commander, duration, commander.anim.die) { }

        public override void Execute()
        {
            trigger.Fire();
            SetDestoryFinal();
        }
    }
}
