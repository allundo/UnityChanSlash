using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(MobAnimator))]
public abstract class MobCommander : MonoBehaviour
{
    public MobAnimator anim { get; protected set; }
    protected MobStatus status;
    protected Pos CurrentPos => map.MapPos(tf.position);

    public Direction dir { get; protected set; }
    public bool IsAutoGuard { get; protected set; } = false;
    public bool IsManualGuard { get; protected set; } = false;

    protected bool isCommandValid = true;
    protected bool IsIdling => currentCommand == null;

    protected bool IsCharactorOn(Pos destPos) => map.GetTile(destPos).IsCharactorOn;
    protected bool IsMovable(Pos destPos) => map.GetTile(destPos).IsEnterable();
    protected bool IsLeapable(Pos destPos) => map.GetTile(destPos).IsLeapable();
    protected bool IsForwardMovable => IsMovable(dir.GetForward(CurrentPos));
    protected bool IsForwardLeapable => IsLeapable(dir.GetForward(CurrentPos));
    protected bool IsBackwardMovable => IsMovable(dir.GetBackward(CurrentPos));
    protected bool IsLeftMovable => IsMovable(dir.GetLeft(CurrentPos));
    protected bool IsRightMovable => IsMovable(dir.GetRight(CurrentPos));
    protected bool IsJumpable => IsForwardLeapable && IsMovable(dir.GetForward(dir.GetForward(CurrentPos)));
    protected bool IsOnPlayer(Pos destPos) => GameManager.Instance.IsOnPlayer(destPos);

    protected Transform tf;

    protected Queue<Command> cmdQueue = new Queue<Command>();
    protected Command currentCommand = null;
    protected Command die = null;

    protected WorldMap map = default;

    protected virtual void Awake()
    {
        anim = GetComponent<MobAnimator>();
    }

    protected virtual void Start()
    {
        status = GetComponent<MobStatus>();

        map = GameManager.Instance.worldMap;

        tf = transform;
        SetPosition(tf);

        SetCommands();
    }

    /// <summary>
    /// This method is called in Start(). By overriding it, you can change start position.
    /// </summary>
    /// <param name="tf"></param>
    protected virtual void SetPosition(Transform tf)
    {
        // TODO: Charactor position should be set by GameManager
        // tf.position = map.GetRespawnPos();
        tf.position = new Vector3(-50, 0, 50);

        dir = new North();
        tf.LookAt(tf.position + dir.LookAt);

        SetOnCharactor(tf.position);
    }

    /// <summary>
    /// This method is called in Start(). By overriding it, you can change commands' behavior.
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



    protected void SetOnCharactor(Vector3 pos)
    {
        map.GetTile(pos).IsCharactorOn = true;
    }
    protected void ResetOnCharactor(Vector3 pos)
    {
        map.GetTile(pos).IsCharactorOn = false;
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
        ResetOnCharactor(tf.position);

        cmdQueue.Clear();
        currentCommand?.Cancel();
        EnqueueDie();
    }

    public virtual void SetSpeed()
    {
        anim.speed.Float = IsIdling ? 0.0f : currentCommand.Speed;
    }

    protected virtual void TurnLeft()
    {
        dir = dir.Left;
    }

    protected virtual void TurnRight()
    {
        dir = dir.Right;
    }

    public virtual void Respawn()
    {
        status.ResetStatus();

        tf.gameObject.SetActive(true);
        isCommandValid = true;

        SetPosition(tf);

        // TODO: Fade-in with custom shader
    }

    protected virtual void Destory()
    {
        currentCommand = null;

        tf.gameObject.SetActive(false);

        // TODO: Fade-out with custom shader
    }

    protected abstract class Command
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
            return commander.tf.DOMove(moveVector, duration)
                .SetRelative()
                .SetEase(Ease.Linear);
        }

        protected Tween GetRotate(int angle = 90)
        {
            return commander.tf.DORotate(new Vector3(0, angle, 0), duration)
                .SetRelative()
                .SetEase(Ease.InCubic);
        }

        protected Sequence GetJumpSequence(Vector3 moveVector, float jumpPower = 1.0f, float edgeTime = 0.3f, float takeoffRate = 0.01f)
        {
            float middleTime = duration - 2 * edgeTime;
            float flyingRate = 1.0f - takeoffRate;

            return DOTween.Sequence()
                .Append(
                    commander.tf.DOMove(moveVector * takeoffRate, edgeTime).SetEase(Ease.OutExpo).SetRelative()
                )
                .Append(
                    commander.tf.DOJump(moveVector * flyingRate, jumpPower, 1, middleTime).SetRelative()
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
            validateTween = DOVirtual.DelayedCall(duration * timing, () => { commander.isCommandValid = true; });
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

    protected abstract class ActionCommand : Command
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
    protected class DieCommand : ActionCommand
    {
        public DieCommand(MobCommander commander, float duration) : base(commander, duration, commander.anim.die) { }

        public override void Execute()
        {
            trigger.Fire();
            SetDestoryFinal();
        }
    }
}
