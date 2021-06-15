using UnityEngine;
using DG.Tweening;

public partial class PlayerCommander : ShieldCommander
{
    [SerializeField] protected ThirdPersonCamera mainCamera = default;

    [SerializeField] protected MobAttack jab = default;
    [SerializeField] protected MobAttack straight = default;
    [SerializeField] protected MobAttack kick = default;

    protected abstract class PlayerCommand : ShieldCommand
    {
        protected PlayerCommander playerCommander;
        protected MapUtil map;
        protected PlayerAnimator anim;
        protected ThirdPersonCamera mainCamera;
        protected HidePool hidePool;

        protected Tween validateTrigger;

        public PlayerCommand(PlayerCommander commander, float duration) : base(commander, duration)
        {
            playerCommander = commander;
            map = commander.map;
            anim = commander.anim as PlayerAnimator;
            mainCamera = commander.mainCamera;
            hidePool = commander.hidePool;
        }

        protected override void SetValidateTimer(float timing = 0.5f)
        {
            validateTween = tweenMove.SetDelayedCall(timing, playerCommander.ValidateInput);
        }

        protected void SetValidateTimer(float timing, float triggerTiming)
        {
            base.SetValidateTimer(timing);
            SetValidateTriggerTimer(triggerTiming);
        }

        protected void SetValidateTriggerTimer(float timing = 0.5f)
        {
            validateTrigger = tweenMove.SetDelayedCall(timing, () => { playerCommander.isTriggerValid = true; });
        }

        public override void Cancel()
        {
            base.Cancel();
            validateTrigger?.Kill();
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
                playerCommander.ValidateInput();
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

            SetValidateTimer(0.95f, 0.5f);
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

            anim.jump.Fire();

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

            SetValidateTimer(0.5f, 0.1f);
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

            SetValidateTimer(0.5f, 0.1f);
            PlayTween(tweenMove.GetRotate(90), () => mainCamera.ResetCamera());
        }
    }
    protected abstract class PlayerAction : PlayerCommand
    {
        protected float timing;
        public PlayerAction(PlayerCommander commander, float duration, float timing = 0.5f) : base(commander, duration)
        {
            this.timing = timing;
        }

        public override void Execute()
        {
            Action();

            SetValidateTimer(timing, timing * 0.5f);
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
        protected MobAttack kick;

        public PlayerAttack(PlayerCommander commander, float duration, float timing = 0.5f) : base(commander, duration, timing)
        {
            jab = commander.jab;
            straight = commander.straight;
            kick = commander.kick;
        }
    }

    protected class PlayerJab : PlayerAttack
    {
        public PlayerJab(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override void Action()
        {
            anim.attack.Fire();
            playingTween = jab.SetAttack(duration);
        }
    }

    protected class PlayerStraight : PlayerAttack
    {
        public PlayerStraight(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override void Action()
        {
            anim.straight.Fire();
            playingTween = straight.SetAttack(duration);
        }
    }
    protected class PlayerKick : PlayerAttack
    {
        public PlayerKick(PlayerCommander commander, float duration) : base(commander, duration) { }

        protected override void Action()
        {
            anim.kick.Fire();
            playingTween = kick.SetAttack(duration);
        }
    }

    protected class PlayerShieldOn : PlayerAttack
    {
        public PlayerShieldOn(PlayerCommander commander, float duration) : base(commander, duration, 0.1f) { }

        protected override void Action()
        {
            anim.shield.Fire();
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
