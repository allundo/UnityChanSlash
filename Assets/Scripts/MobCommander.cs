using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public abstract class MobCommander : MonoBehaviour
{
    protected Animator anim;
    protected Pos CurrentPos => map.MapPos(tf.position);

    protected Direction dir;

    protected bool isCommandValid = true;
    protected bool IsIdling => currentCommand == null;

    protected bool IsMovable(Pos descPos) => map.GetTile(descPos).IsEnterable();
    protected bool IsLeapable(Pos descPos) => map.GetTile(descPos).IsLeapable();
    protected bool IsForwardMovable => IsMovable(dir.GetForward(CurrentPos));
    protected bool IsForwardLeapable => IsLeapable(dir.GetForward(CurrentPos));
    protected bool IsBackwardMovable => IsMovable(dir.GetBackward(CurrentPos));
    protected bool IsLeftMovable => IsMovable(dir.GetLeft(CurrentPos));
    protected bool IsRightMovable => IsMovable(dir.GetRight(CurrentPos));
    protected bool IsJumpable => IsForwardLeapable && IsMovable(dir.GetForward(dir.GetForward(CurrentPos)));

    protected Transform tf;

    protected Queue<Command> cmdQueue = new Queue<Command>();
    protected Command currentCommand = null;

    protected WorldMap map = default;

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();

        map = GameManager.Instance.worldMap;

        tf = transform;
        SetPosition(tf);
        SetCommands();
    }

    protected virtual void SetPosition(Transform tf)
    {
        // TODO: Charactor position should be set by GameManager
        tf.position = map.InitPos + new Vector3(2.5f, 0, 0);

        dir = new East();
        tf.LookAt(tf.position + dir.LookAt);

        SetOnCharactor(tf.position);
    }

    protected abstract void SetCommands();

    protected void SetOnCharactor(Vector3 pos)
    {
        map.GetTile(pos).IsCharactorOn = true;
    }
    protected void ResetOnCharactor(Vector3 pos)
    {
        map.GetTile(pos).IsCharactorOn = false;
    }

    public void InputCommand()
    {
        if (!isCommandValid)
        {
            return;
        }

        Command cmd = GetCommand();

        if (cmd != null)
        {
            cmdQueue.Enqueue(cmd);
            isCommandValid = false;

            if (IsIdling)
            {
                DispatchCommand();
            }
        }
    }

    public bool DispatchCommand()
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

    public virtual void SetSpeed()
    {
        anim.SetFloat("Speed", (IsIdling ? 0.0f : currentCommand.Speed));
    }

    protected virtual void TurnLeft()
    {
        dir = dir.Left;
    }

    protected virtual void TurnRight()
    {
        dir = dir.Right;
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

        public abstract void Execute();
        public virtual float Speed => 0.0f;
        public virtual float RSpeed => 0.0f;

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

        protected void PlayTweenMove(Tween move, Action OnComplete = null)
        {
            OnComplete = OnComplete ?? (() => { });

            move.OnComplete(() =>
            {
                OnComplete();
                commander.DispatchCommand();
            }).Play();
        }
    }

    protected abstract class ActionCommand : Command
    {
        public ActionCommand(MobCommander commander, float duration, string animName) : base(commander, duration)
        {
            this.animName = animName;
        }

        protected string animName;

        public override void Execute()
        {
            commander.anim.SetTrigger(animName);

            DOVirtual.DelayedCall(duration * 0.5f, () => { commander.isCommandValid = true; });
            DOVirtual.DelayedCall(duration, () => { commander.DispatchCommand(); });
        }
    }

    protected class AttackCommand : ActionCommand
    {
        public AttackCommand(MobCommander commander, float duration) : base(commander, duration, "Attack") { }
    }
}