using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(MobControl))]
[RequireComponent(typeof(Animator))]
public class Commander : MonoBehaviour
{
    protected MobControl character;
    protected Pos CurrentPos => map.MapPos(tf.position);
    protected Animator anim;

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

    [SerializeField] protected ThirdPersonCamera mainCamera = default;
    protected WorldMap map = default;

    protected void Start()
    {
        character = GetComponent<MobControl>();
        anim = GetComponent<Animator>();

        map = GameManager.Instance.worldMap;

        tf = character.transform;

        // TODO: Charactor position should be set by GameManager
        (float x, float z) pos = map.InitPos;
        tf.position = new Vector3(pos.x, 0, pos.z);

        dir = map.InitDir;
        tf.LookAt(tf.position + dir.LookAt);

        Debug.Log("Position: " + tf.position);

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
        if (Input.GetButtonDown("Jump")) return Command.jump;
        if (Input.GetButtonDown("Fire2")) return Command.turnL;
        if (Input.GetButtonDown("Fire3")) return Command.turnR;
        if (Input.GetButtonDown("Fire1")) return Command.attack;
        if (Input.GetButtonDown("Submit")) return Command.handle;

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (v > 0) return Command.forward;
        if (v < 0) return Command.back;

        if (h > 0) return Command.right;
        if (h < 0) return Command.left;

        return null;
    }

    public float GetSpeed()
    {
        if (IsIdling) return 0.0f;

        return currentCommand.Speed;
    }

    public float GetRSpeed()
    {
        if (IsIdling) return 0.0f;

        return currentCommand.RSpeed;
    }

    protected void TurnLeft()
    {
        mainCamera.TurnLeft();
        dir = dir.Left;
    }

    protected void TurnRight()
    {
        mainCamera.TurnRight();
        dir = dir.Right;
    }

    protected void ResetCamera()
    {
        mainCamera.ResetCamera();
    }

    protected abstract class Command
    {
        protected const float TILE_UNIT = 2.5f;

        protected float duration;

        public static Commander commander;

        public static Command forward;
        public static Command back;
        public static Command right;
        public static Command left;
        public static Command jump;
        public static Command turnL;
        public static Command turnR;
        public static Command attack;
        public static Command handle;

        public static void Init(Commander commander, float baseDuration = 0.6f)
        {
            Command.commander = commander;

            forward = new ForwardCommand(baseDuration);
            back = new BackCommand(baseDuration * 1.2f);
            right = new RightCommand(baseDuration * 1.2f);
            left = new LeftCommand(baseDuration * 1.2f);
            jump = new JumpCommand(baseDuration * 2);
            turnL = new TurnLCommand(baseDuration * 0.5f);
            turnR = new TurnRCommand(baseDuration * 0.5f);
            attack = new AttackCommand(baseDuration * 2);
            handle = new HandleCommand(baseDuration);
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
            Debug.Log("IsMovable: " + IsMovable);

            if (!IsMovable)
            {
                commander.isCommandValid = true;
                commander.DispatchCommand();
                return;
            }

            PlayTweenMove(GetLinearMove(Dest));

            DOVirtual.DelayedCall(duration * 0.5f, () => { commander.isCommandValid = true; });
        }
    }

    protected class ForwardCommand : MoveCommand
    {
        public ForwardCommand(float duration) : base(duration) { }

        protected override bool IsMovable => commander.IsForwardMovable;
        protected override Vector3 Dest => commander.dir.LookAt * TILE_UNIT;
        public override float Speed => TILE_UNIT / duration;
    }

    protected class BackCommand : MoveCommand
    {
        public BackCommand(float duration) : base(duration) { }

        protected override bool IsMovable => commander.IsBackwardMovable;
        protected override Vector3 Dest => -commander.dir.LookAt * TILE_UNIT;
        public override float Speed => -TILE_UNIT / duration;
    }

    protected class RightCommand : MoveCommand
    {
        public RightCommand(float duration) : base(duration) { }

        protected override bool IsMovable => commander.IsRightMovable;
        protected override Vector3 Dest => Quaternion.Euler(0, 90, 0) * commander.dir.LookAt * TILE_UNIT;
        public override float RSpeed => TILE_UNIT / duration;
    }

    protected class LeftCommand : MoveCommand
    {
        public LeftCommand(float duration) : base(duration) { }

        protected override bool IsMovable => commander.IsLeftMovable;
        protected override Vector3 Dest => Quaternion.Euler(0, -90, 0) * commander.dir.LookAt * TILE_UNIT;
        public override float RSpeed => -TILE_UNIT / duration;
    }

    protected class JumpCommand : Command
    {
        public JumpCommand(float duration) : base(duration) { }

        public override void Execute()
        {
            Debug.Log("Jump");

            Vector3 jump = commander.dir.LookAt * TILE_UNIT *
                (commander.IsJumpable ? 2 : commander.IsForwardMovable ? 1 : 0);
            PlayTweenMove(GetJumpSequence(jump));
            commander.anim.SetTrigger("Jump");

            DOVirtual.DelayedCall(duration * 0.5f, () => { commander.isCommandValid = true; });
        }
    }

    protected class TurnLCommand : Command
    {
        public TurnLCommand(float duration) : base(duration) { }

        public override void Execute()
        {
            PlayTweenMove(GetRotate(-90), () => commander.ResetCamera());
            commander.TurnLeft();
            commander.anim.SetTrigger("TurnL");

            DOVirtual.DelayedCall(duration * 0.5f, () => { commander.isCommandValid = true; });
        }
    }

    protected class TurnRCommand : Command
    {
        public TurnRCommand(float duration) : base(duration) { }

        public override void Execute()
        {
            PlayTweenMove(GetRotate(90), () => commander.ResetCamera());
            commander.TurnRight();
            commander.anim.SetTrigger("TurnR");

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
    protected class HandleCommand : ActionCommand
    {
        public HandleCommand(float duration) : base(duration, "Handle") { }
    }
}