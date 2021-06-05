using UnityEngine;

public partial class PlayerCommander : ShieldCommander
{
    [SerializeField] protected MobAttack jab = default;
    [SerializeField] protected MobAttack straight = default;

    protected abstract class PlayerCommand : ShieldCommand
    {
        protected PlayerCommander playerCommander;
        protected MapUtil map;
        protected PlayerAnimator anim;
        protected ThirdPersonCamera mainCamera;
        protected HidePool hidePool;

        public PlayerCommand(PlayerCommander commander, float duration) : base(commander, duration)
        {
            playerCommander = commander;
            map = commander.map;
            anim = commander.anim as PlayerAnimator;
            mainCamera = commander.mainCamera;
            hidePool = commander.hidePool;
        }
    }

    protected abstract class MoveCommand : PlayerCommand
    {
        public MoveCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected abstract bool IsMovable { get; }
        protected abstract Vector3 Dest { get; }
        protected Vector3 startPos = default;

        protected void SetSpeed()
        {
            anim.speed.Float = Speed;
            anim.rSpeed.Float = RSpeed;
        }

        protected void ResetSpeed()
        {
            anim.speed.Float = 0.0f;
            anim.rSpeed.Float = 0.0f;
        }

        public override void Cancel()
        {
            base.Cancel();
            map.ResetOnCharactor(startPos + Dest);
        }

        public override void Execute()
        {
            if (!IsMovable)
            {
                playerCommander.isCommandValid = true;
                playerCommander.DispatchCommand();
                return;
            }

            startPos = map.CurrentVec3Pos;
            map.SetOnCharactor(startPos + Dest);
            map.ResetOnCharactor(startPos);

            SetSpeed();
            PlayTween(tweenMove.GetLinearMove(Dest), () =>
            {
                hidePool.Move();
                ResetSpeed();
            });

            SetValidateTimer(0.95f);
        }
    }

    protected class ForwardCommand : MoveCommand
    {
        public ForwardCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override bool IsMovable => map.IsForwardMovable;
        protected override Vector3 Dest => map.GetForwardVector();
        public override float Speed => TILE_UNIT / duration;
    }

    protected class BackCommand : MoveCommand
    {
        public BackCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override bool IsMovable => map.IsBackwardMovable;
        protected override Vector3 Dest => map.GetBackwardVector();
        public override float Speed => -TILE_UNIT / duration;
    }

    protected class RightCommand : MoveCommand
    {
        public RightCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override bool IsMovable => map.IsRightMovable;
        protected override Vector3 Dest => map.GetRightVector();
        public override float RSpeed => TILE_UNIT / duration;
    }

    protected class LeftCommand : MoveCommand
    {
        public LeftCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override bool IsMovable => map.IsLeftMovable;
        protected override Vector3 Dest => map.GetLeftVector();
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
            map.ResetOnCharactor(startPos + dest);
        }

        public override void Execute()
        {
            Debug.Log("Jump");

            startPos = map.CurrentVec3Pos;

            int distance = map.IsJumpable ? 2 : map.IsForwardMovable ? 1 : 0;
            dest = map.GetForwardVector(distance);

            map.SetOnCharactor(startPos + dest);
            map.ResetOnCharactor(startPos);

            anim.jump.Fire(duration);

            // 2マス進む場合は途中で天井の状態を更新
            if (distance == 2)
            {
                tweenMove.SetDelayedCall(0.4f, () => hidePool.Move());
            }

            SetValidateTimer();

            PlayTween(tweenMove.GetJumpSequence(dest), () =>
            {
                if (distance > 0) hidePool.Move();
            });
        }
    }

    protected class TurnLCommand : PlayerCommand
    {
        public TurnLCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            map.TurnLeft();
            mainCamera.TurnLeft();
            anim.turnL.Fire();

            SetValidateTimer();
            PlayTween(tweenMove.GetRotate(-90), () => mainCamera.ResetCamera());
        }
    }

    protected class TurnRCommand : PlayerCommand
    {
        public TurnRCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            map.TurnRight();
            mainCamera.TurnRight();
            anim.turnR.Fire();

            SetValidateTimer();
            PlayTween(tweenMove.GetRotate(90), () => mainCamera.ResetCamera());
        }
    }
    protected abstract class PlayerAction : PlayerCommand
    {
        public PlayerAction(PlayerCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            Action();

            SetValidateTimer();
            SetDispatchFinal();
        }

        protected abstract void Action();
    }

    protected class PlayerHandle : PlayerAction
    {
        public PlayerHandle(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override void Action()
        {
            anim.handle.Fire();
        }
    }

    protected abstract class PlayerAttack : PlayerAction
    {
        protected MobAttack jab;
        protected MobAttack straight;

        public PlayerAttack(PlayerCommander commander, float duration) : base(commander, duration)
        {
            jab = commander.jab;
            straight = commander.straight;
        }
    }

    protected class PlayerJab : PlayerAttack
    {
        public PlayerJab(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override void Action()
        {
            anim.attack.Fire();
            jab.SetAttack();
        }
    }

    protected class PlayerStraight : PlayerAttack
    {
        public PlayerStraight(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override void Action()
        {
            anim.straight.Fire();
            straight.SetAttack();
        }
    }

    protected class PlayerDie : PlayerCommand
    {
        public PlayerDie(PlayerCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            anim.dieEx.Fire();
            SetDestoryFinal();
        }
    }
}
