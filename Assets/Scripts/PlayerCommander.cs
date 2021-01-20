using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Animator))]
public class PlayerCommander : MobCommander
{
    [SerializeField] protected ThirdPersonCamera mainCamera = default;

    protected Command forward;
    protected Command turnL;
    protected Command turnR;
    protected Command attack;
    protected Command back;
    protected Command right;
    protected Command left;
    protected Command jump;
    protected Command handle;

    protected override void SetPosition(Transform tf)
    {
        // TODO: Charactor position should be set by GameManager
        tf.position = map.InitPos;

        dir = map.InitDir;
        tf.LookAt(tf.position + dir.LookAt);

        SetOnCharactor(tf.position);
        MapRenderer.Instance.RedrawHidePlates(tf.position);
    }

    protected override void SetCommands()
    {
        forward = new ForwardCommand(this, 1.0f);
        turnL = new TurnLCommand(this, 0.5f);
        turnR = new TurnRCommand(this, 0.5f);
        attack = new AttackCommand(this, 2.0f);

        back = new BackCommand(this, 1.2f);
        right = new RightCommand(this, 1.2f);
        left = new LeftCommand(this, 1.2f);
        jump = new JumpCommand(this, 2.0f);
        handle = new HandleCommand(this, 1.0f);
        die = new DieCommand(this, 10.0f);
    }

    protected override Command GetCommand()
    {
        if (Input.GetButtonDown("Jump")) return jump;
        if (Input.GetButtonDown("Fire2")) return turnL;
        if (Input.GetButtonDown("Fire3")) return turnR;
        if (Input.GetButtonDown("Fire1")) return attack;
        if (Input.GetButtonDown("Submit")) return handle;

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (v > 0) return forward;
        if (v < 0) return back;

        if (h > 0) return right;
        if (h < 0) return left;

        return null;
    }

    public override void SetSpeed()
    {
        anim.SetFloat("Speed", IsIdling ? 0.0f : currentCommand.Speed);
        anim.SetFloat("RSpeed", IsIdling ? 0.0f : currentCommand.RSpeed);
    }

    protected override void TurnLeft()
    {
        mainCamera.TurnLeft();
        dir = dir.Left;
    }

    protected override void TurnRight()
    {
        mainCamera.TurnRight();
        dir = dir.Right;
    }

    protected void ResetCamera()
    {
        mainCamera.ResetCamera();
    }

    protected abstract class PlayerCommand : Command
    {
        protected PlayerCommander playerCommander;

        public PlayerCommand(PlayerCommander commander, float duration) : base(commander, duration)
        {
            playerCommander = commander;
        }
    }

    protected abstract class MoveCommand : PlayerCommand
    {
        public MoveCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected abstract bool IsMovable { get; }
        protected abstract Vector3 Dest { get; }
        protected Vector3 startPos = default;
        public override void Cancel()
        {
            base.Cancel();
            playerCommander.ResetOnCharactor(startPos + Dest);
        }

        public override void Execute()
        {
            if (!IsMovable)
            {
                playerCommander.isCommandValid = true;
                playerCommander.DispatchCommand();
                return;
            }

            startPos = playerCommander.tf.position;
            playerCommander.SetOnCharactor(startPos + Dest);

            PlayTweenMove(
                JoinTweens(
                    GetLinearMove(Dest),
                    DOVirtual.DelayedCall(duration * 0.25f, () => { playerCommander.ResetOnCharactor(startPos); })
                ),
                () => { MapRenderer.Instance.MoveHidePlates(playerCommander.tf.position); }
            );

            SetValidateTimer(0.95f);
        }
    }

    protected class ForwardCommand : MoveCommand
    {
        public ForwardCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override bool IsMovable => playerCommander.IsForwardMovable;
        protected override Vector3 Dest => playerCommander.dir.LookAt * TILE_UNIT;
        public override float Speed => TILE_UNIT / duration;
    }

    protected class BackCommand : MoveCommand
    {
        public BackCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override bool IsMovable => playerCommander.IsBackwardMovable;
        protected override Vector3 Dest => -playerCommander.dir.LookAt * TILE_UNIT;
        public override float Speed => -TILE_UNIT / duration;
    }

    protected class RightCommand : MoveCommand
    {
        public RightCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override bool IsMovable => playerCommander.IsRightMovable;
        protected override Vector3 Dest => Quaternion.Euler(0, 90, 0) * playerCommander.dir.LookAt * TILE_UNIT;
        public override float RSpeed => TILE_UNIT / duration;
    }

    protected class LeftCommand : MoveCommand
    {
        public LeftCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override bool IsMovable => playerCommander.IsLeftMovable;
        protected override Vector3 Dest => Quaternion.Euler(0, -90, 0) * playerCommander.dir.LookAt * TILE_UNIT;
        public override float RSpeed => -TILE_UNIT / duration;
    }

    protected class JumpCommand : PlayerCommand
    {
        public JumpCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected Vector3 dest = default;
        protected Vector3 startPos = default;

        public override void Cancel()
        {
            base.Cancel();
            playerCommander.ResetOnCharactor(startPos + dest);
        }

        public override void Execute()
        {
            Debug.Log("Jump");

            startPos = playerCommander.tf.position;

            int distance = (playerCommander.IsJumpable ? 2 : playerCommander.IsForwardMovable ? 1 : 0);
            dest = playerCommander.dir.LookAt * TILE_UNIT * distance;

            playerCommander.SetOnCharactor(startPos + dest);
            playerCommander.anim.SetTrigger("Jump");

            if (distance > 0)
            {
                PlayTweenMove(
                    JoinTweens(
                        GetJumpSequence(dest),
                        DOVirtual.DelayedCall(duration * 0.25f, () => { playerCommander.ResetOnCharactor(startPos); })
                    ),
                    () => MapRenderer.Instance.MoveHidePlates(playerCommander.tf.position)
                );
            }
            else
            {
                PlayTweenMove(GetJumpSequence(dest));
            }

            if (distance == 2)
            {
                DOVirtual.DelayedCall(duration * 0.4f, () =>
                {
                    MapRenderer.Instance.MoveHidePlates(playerCommander.tf.position);
                });
            }

            SetValidateTimer();
        }
    }

    protected class TurnLCommand : PlayerCommand
    {
        public TurnLCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            PlayTweenMove(GetRotate(-90), () => playerCommander.ResetCamera());
            playerCommander.TurnLeft();
            playerCommander.anim.SetTrigger("TurnL");

            SetValidateTimer();
        }
    }

    protected class TurnRCommand : PlayerCommand
    {
        public TurnRCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            PlayTweenMove(GetRotate(90), () => playerCommander.ResetCamera());
            playerCommander.TurnRight();
            playerCommander.anim.SetTrigger("TurnR");

            SetValidateTimer();
        }
    }

    protected class HandleCommand : ActionCommand
    {
        public HandleCommand(PlayerCommander commander, float duration) : base(commander, duration, "Handle") { }
    }
}