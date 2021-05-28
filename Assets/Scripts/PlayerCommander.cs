using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

[RequireComponent(typeof(UnityChanAnimeHandler))]
public class PlayerCommander : ShieldCommander
{
    public override bool IsShieldEnable => IsIdling || currentInput == guard;
    protected bool IsFaceToEnemy => map.IsCharactorOn(map.GetForward);

    // TODO: Rename variable of InputManagers for consistency. Those names should be used by Command.
    // TODO: InputManager might be replaced with uGUI controllers.
    protected InputManager currentInput = new InputManager(null);
    protected InputManager forward;
    protected InputManager turnL;
    protected InputManager turnR;
    protected InputManager attack;
    protected InputManager guard;
    protected InputManager back;
    protected InputManager right;
    protected InputManager left;
    protected InputManager jump;
    protected InputManager handle;

    protected override void SetCommands()
    {
        forward = new FrontInput(new ForwardCommand(this, 1.0f), new PlayerAttack(this, 2.0f), new PlayerHandle(this, 1.0f));
        turnL = new TriggerInput(new TurnLCommand(this, 0.5f));
        turnR = new TriggerInput(new TurnRCommand(this, 0.5f));
        attack = new TriggerInput(new PlayerAttack(this, 0.6f));

        back = new InputManager(new BackCommand(this, 1.2f));
        right = new InputManager(new RightCommand(this, 1.2f));
        left = new InputManager(new LeftCommand(this, 1.2f));
        jump = new TriggerInput(new JumpCommand(this, 2.0f));
        handle = new InputManager(new PlayerHandle(this, 1.0f));
        guard = new InputManager(new GuardCommand(this, 0.2f));

        die = new DieCommand(this, 10.0f);
    }

    private void OnEnable()
    {
        TouchSimulation.Enable();
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        TouchSimulation.Disable();
        EnhancedTouchSupport.Disable();
    }

    protected override Command GetCommand()
    {
        ReadOnlyArray<Touch> activeTouches = Touch.activeTouches;
        if (activeTouches.Count == 0) return null;

        Touch latestTouch = default;

        foreach (Touch touch in Touch.activeTouches)
        {
            if (latestTouch.finger == null || touch.startTime > latestTouch.startTime)
            {
                latestTouch = touch;
            }
        }

        switch (latestTouch.phase)
        {
            case TouchPhase.Began:
                currentInput = GetNewInput(latestTouch.screenPosition);

                return currentInput.FingerDown();

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (currentInput == forward && currentInput.isPressed)
                {
                    return currentInput.FingerMove(latestTouch.delta);
                }
                if (currentInput == attack && currentInput.isPressed)
                {
                    return currentInput.FingerMove(latestTouch.delta);
                }

                currentInput = GetNewInput(latestTouch.screenPosition);
                return currentInput.FingerMove(Vector2.zero);

            case TouchPhase.Ended:
                return currentInput.FingerUp();

            default:
                return null;

        }
    }

    private InputManager GetNewInput(Vector2 screenPos)
    {
        InputManager ret = null;

        if (IsInRect(screenPos, -0.18f, 0.6f, 0.18f, 0.9f))
        {
            ret = jump;
        }
        if (IsInRect(screenPos, -0.7f, 0.16f, -0.18f, 0.6f))
        {
            ret = turnL;
        }
        if (IsInRect(screenPos, 0.18f, 0.16f, 0.7f, 0.6f))
        {
            ret = turnR;
        }

        if (IsInRect(screenPos, -0.18f, 0.16f, 0.18f, 0.34f))
        {
            ret = IsAutoGuard ? attack : guard;
        }
        if (IsInRect(screenPos, -0.18f, 0.34f, 0.18f, 0.6f))
        {
            ret = forward;
        }
        if (IsInRect(screenPos, -0.7f, 0, -0.18f, 0.16f))
        {
            ret = left;
        }
        if (IsInRect(screenPos, 0.18f, 0, 0.9f, 0.16f))
        {
            ret = right;
        }
        if (IsInRect(screenPos, -0.18f, 0, 0.18f, 0.16f))
        {
            ret = back;
        }

        if (ret != null)
        {
            currentInput.Reset();
            return ret;
        }

        return currentInput;
    }

    private bool IsInRect(Vector2 pos, Vector2 lowerLeft, Vector2 upperRight)
    {
        return IsInRect(pos, lowerLeft.x, lowerLeft.y, upperRight.x, upperRight.y);
    }

    private bool IsInRect(Vector2 pos, float left, float bottom, float right, float top)
    {
        float centerX = Screen.width / 2.0f / Screen.height;
        float scaledPosX = pos.x / Screen.height;
        float scaledPosY = pos.y / Screen.height;
        return scaledPosX >= left + centerX && scaledPosX < right + centerX && scaledPosY >= bottom && scaledPosY < top;
    }

    protected override void InputCommand()
    {
        Command cmd = GetCommand();

        if (!isCommandValid)
        {
            return;
        }

        if (cmd != null)
        {
            EnqueueCommand(cmd, IsIdling);
        }
        else if (IsIdling)
        {
            SetEnemyDetected(IsFaceToEnemy);
        }
    }

    protected class InputManager
    {
        public bool isPressed { get; protected set; } = false;
        protected Command mainCommand;

        public InputManager(Command mainCommand)
        {
            this.mainCommand = mainCommand;
        }

        public virtual Command FingerDown()
        {
            isPressed = true;
            return mainCommand;
        }

        public virtual Command FingerMove(Vector2 moveVec)
        {
            return mainCommand;
        }
        public virtual Command FingerUp()
        {
            isPressed = false;
            return mainCommand;
        }

        public virtual void Reset()
        {
            isPressed = false;
        }
    }

    protected class TriggerInput : InputManager
    {
        public TriggerInput(Command mainCommand) : base(mainCommand) { }
        public override Command FingerMove(Vector2 moveVec)
        {
            return null;
        }
        public override Command FingerUp()
        {
            isPressed = false;
            return null;
        }
    }

    protected class FrontInput : InputManager
    {
        protected bool isStationary = false;
        protected bool isPulling = false;
        protected bool isPushing = false;
        protected bool isRight = false;
        protected bool isLeft = false;
        protected bool isExecuted = false;

        protected PlayerAttack attack;
        protected PlayerHandle handle;

        public FrontInput(Command mainCommand, PlayerAttack attack, PlayerHandle handle) : base(mainCommand)
        {
            this.attack = attack;
            this.handle = handle;
        }

        public override Command FingerDown()
        {
            isPressed = true;
            return null;
        }

        public override Command FingerMove(Vector2 moveVec)
        {
            if (!isPressed) return mainCommand;

            float absX = Mathf.Abs(moveVec.x);
            float absY = Mathf.Abs(moveVec.y);

            if (absX < 0.00001f && absY < 0.00001f)
            {
                // Tap command
                if (isStationary)
                {
                    isExecuted = true;
                    isPressed = false;
                    return mainCommand;
                }
                else
                {
                    isStationary = true;
                    return null;
                }
            }

            if (absX < absY)
            {
                // Vertical command
                if (moveVec.y < 0)
                {
                    // Pulling command
                    if (absX * 2 < absY)
                    {
                        if (isPulling)
                        {
                            isPressed = false;
                            return handle;
                        }
                        else
                        {
                            isStationary = false;
                            isPulling = true;
                            return null;
                        }
                    }
                }
                else
                {
                    // Pushing command
                    if (absX * 2 < absY)
                    {
                        if (isPushing)
                        {
                            isPressed = false;
                            return attack;
                        }
                        else
                        {
                            isStationary = false;
                            isPushing = true;
                            return null;
                        }
                    }
                }
            }
            else
            {
                // Horizontal command
                if (moveVec.x < 0)
                {
                    if (absY * 2 < absX)
                    {
                        // Door opening command
                        if (isLeft)
                        {
                            isPressed = false;
                            return handle;
                        }
                        else
                        {
                            isStationary = false;
                            isLeft = true;
                            return null;
                        }
                    }
                }
                else
                {
                    if (absY * 2 < absX)
                    {
                        // Door opening command
                        if (isRight)
                        {
                            isPressed = false;
                            return handle;
                        }
                        else
                        {
                            isStationary = false;
                            isRight = true;
                            return null;
                        }
                    }
                }
            }
            return null;
        }

        public override Command FingerUp()
        {
            if (isPressed)
            {
                Reset();
                return mainCommand;
            }
            return null;
        }

        public override void Reset()
        {
            isPressed = isStationary = isPulling = isPushing = isRight = isLeft = false;
        }
    }

    protected abstract class PlayerCommand : ShieldCommand
    {
        protected PlayerCommander playerCommander;
        protected PlayerMapUtil map;
        protected UnityChanAnimeHandler anim;

        public PlayerCommand(PlayerCommander commander, float duration) : base(commander, duration)
        {
            playerCommander = commander;
            map = playerCommander.map as PlayerMapUtil;
            anim = playerCommander.anim as UnityChanAnimeHandler;
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
                map.MoveHidePlates();
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

            anim.jump.Fire();

            // 2マス進む場合は途中で天井の状態を更新
            if (distance == 2)
            {
                tweenMove.SetDelayedCall(0.4f, () => map.MoveHidePlates());
            }

            SetValidateTimer();

            PlayTween(tweenMove.GetJumpSequence(dest), () =>
            {
                if (distance > 0) map.MoveHidePlates();
            });
        }
    }

    protected class TurnLCommand : PlayerCommand
    {
        public TurnLCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            map.TurnLeft();
            anim.turnL.Fire();

            SetValidateTimer();
            PlayTween(tweenMove.GetRotate(-90), () => map.ResetCamera());
        }
    }

    protected class TurnRCommand : PlayerCommand
    {
        public TurnRCommand(PlayerCommander commander, float duration) : base(commander, duration) { }

        public override void Execute()
        {
            map.TurnRight();
            anim.turnR.Fire();

            SetValidateTimer();
            PlayTween(tweenMove.GetRotate(90), () => map.ResetCamera());
        }
    }
    protected class PlayerAction : PlayerCommand
    {
        protected ShieldAnimator.TriggerEx trigger;

        public PlayerAction(PlayerCommander commander, float duration, ShieldAnimator.TriggerEx trigger) : base(commander, duration)
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

    protected class PlayerHandle : PlayerCommand
    {
        public PlayerHandle(PlayerCommander commander, float duration) : base(commander, duration) { }
        public override void Execute()
        {
            anim.handle.Fire();

            SetValidateTimer();
            SetDispatchFinal();
        }
    }

    protected class PlayerAttack : PlayerCommand
    {
        public PlayerAttack(PlayerCommander commander, float duration) : base(commander, duration) { }
        public override void Execute()
        {
            anim.attack.Fire();

            SetValidateTimer();
            SetDispatchFinal();
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
