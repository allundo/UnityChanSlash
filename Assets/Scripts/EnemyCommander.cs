using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyControl))]
[RequireComponent(typeof(Animator))]
public class EnemyCommander : MonoBehaviour
{
    protected EnemyControl character;
    protected Animator anim;

    protected Random random;
    protected Pos CurrentPos => map.MapPos(tf.position);

    protected Direction dir;

    protected bool isCommandValid = true;
    protected bool IsIdling => currentCommand == null;

    protected bool IsMovable(Pos descPos) => map.GetTile(descPos).IsEnterable();
    protected bool IsLeapable(Pos descPos) => map.GetTile(descPos).IsLeapable();
    protected bool IsForwardMovable => IsMovable(dir.GetForward(CurrentPos));

    protected Transform tf;

    protected Queue<Command> cmdQueue = new Queue<Command>();
    protected Command currentCommand = null;
    protected WorldMap map = default;

    protected void Start()
    {
        character = GetComponent<EnemyControl>();
        anim = GetComponent<Animator>();

        map = GameManager.Instance.worldMap;

        tf = character.transform;

        // TODO: Charactor position should be set by GameManager
        tf.position = map.InitPos + new Vector3(2.5f, 0, 0);

        dir = new East();
        tf.LookAt(tf.position + dir.LookAt);

        Command.Init(this);
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

    private Command GetCommand()
    {
        switch (Random.Range(0, 4))
        {
            case 0:
                return Command.forward;
            case 1:
                return Command.turnL;
            case 2:
                return Command.turnR;
            case 3:
                return Command.attack;
        }
        /*
        if (Input.GetButtonDown("Fire2")) return Command.turnL;
        if (Input.GetButtonDown("Fire3")) return Command.turnR;
        if (Input.GetButtonDown("Fire1")) return Command.attack;

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (v > 0) return Command.forward;
        if (v < 0) return Command.back;

        if (h > 0) return Command.right;
        if (h < 0) return Command.left;

        return null;
        */
        return Command.forward;
    }

    public void SetSpeed()
    {
        anim.SetFloat("Speed", (IsIdling ? 0.0f : currentCommand.Speed));
    }

    protected void TurnLeft()
    {
        dir = dir.Left;
    }

    protected void TurnRight()
    {
        dir = dir.Right;
    }

    protected abstract class Command
    {
        protected const float TILE_UNIT = 2.5f;

        protected float duration;

        public static EnemyCommander commander;

        public static Command forward;
        public static Command turnL;
        public static Command turnR;
        public static Command attack;

        public static void Init(EnemyCommander commander, float baseDuration = 0.6f)
        {
            Command.commander = commander;

            forward = new ForwardCommand(baseDuration * 2.0f);
            turnL = new TurnLCommand(baseDuration * 0.1f);
            turnR = new TurnRCommand(baseDuration * 0.1f);
            attack = new AttackCommand(baseDuration * 2.0f);
        }

        public Command(float duration)
        {
            this.duration = duration;
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

    protected abstract class MoveCommand : Command
    {
        public MoveCommand(float duration) : base(duration) { }

        abstract protected bool IsMovable { get; }
        abstract protected Vector3 Dest { get; }

        public override void Execute()
        {
            if (!IsMovable)
            {
                commander.isCommandValid = true;
                commander.DispatchCommand();
                return;
            }

            PlayTweenMove(GetLinearMove(Dest));

            DOVirtual.DelayedCall(duration * 0.95f, () => { commander.isCommandValid = true; });
        }
    }

    protected class ForwardCommand : MoveCommand
    {
        public ForwardCommand(float duration) : base(duration) { }

        protected override bool IsMovable => commander.IsForwardMovable;
        protected override Vector3 Dest => commander.dir.LookAt * TILE_UNIT;
        public override float Speed => TILE_UNIT / duration;
    }


    protected class TurnLCommand : Command
    {
        public TurnLCommand(float duration) : base(duration) { }

        public override void Execute()
        {
            PlayTweenMove(GetRotate(-90));
            commander.TurnLeft();

            DOVirtual.DelayedCall(duration * 0.5f, () => { commander.isCommandValid = true; });
        }
    }

    protected class TurnRCommand : Command
    {
        public TurnRCommand(float duration) : base(duration) { }

        public override void Execute()
        {
            PlayTweenMove(GetRotate(90));
            commander.TurnRight();

            DOVirtual.DelayedCall(duration * 0.5f, () => { commander.isCommandValid = true; });
        }
    }

    protected abstract class ActionCommand : Command
    {
        public ActionCommand(float duration, string animName) : base(duration)
        {
            this.animName = animName;
        }

        protected string animName;

        public override void Execute()
        {
            Debug.Log(animName);
            commander.anim.SetTrigger(animName);

            DOVirtual.DelayedCall(duration * 0.5f, () => { commander.isCommandValid = true; });
            DOVirtual.DelayedCall(duration, () => { commander.DispatchCommand(); });
        }
    }

    protected class AttackCommand : ActionCommand
    {
        public AttackCommand(float duration) : base(duration, "Attack") { }
    }
}