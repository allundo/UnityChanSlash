using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public partial class PlayerCommander : ShieldCommander
{
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

    protected class CommandInput
    {
        private GuardState guardState;

        // TODO: Rename variable of InputManagers for consistency. Those names should be used by Command.
        // TODO: InputManager might be replaced with uGUI controllers.
        private InputManager currentInput = new InputManager(null);
        private InputManager forward;
        private InputManager turnL;
        private InputManager turnR;
        private InputManager attack;
        private InputManager guard;
        private InputManager back;
        private InputManager right;
        private InputManager left;
        private InputManager jump;
        private InputManager handle;

        public CommandInput(PlayerCommander commander)
        {
            guardState = commander.guardState;
            SetInputManagers(commander);
        }

        public void SetInputManagers(PlayerCommander commander)
        {
            forward = new FrontInput(new ForwardCommand(commander, 1.0f), new PlayerAttack(commander, 2.0f), new PlayerHandle(commander, 1.0f));
            turnL = new TriggerInput(new TurnLCommand(commander, 0.5f));
            turnR = new TriggerInput(new TurnRCommand(commander, 0.5f));
            attack = new TriggerInput(new PlayerAttack(commander, 0.6f));

            back = new InputManager(new BackCommand(commander, 1.2f));
            right = new InputManager(new RightCommand(commander, 1.2f));
            left = new InputManager(new LeftCommand(commander, 1.2f));
            jump = new TriggerInput(new JumpCommand(commander, 2.0f));
            handle = new InputManager(new PlayerHandle(commander, 1.0f));
            guard = new InputManager(new GuardCommand(commander, 0.2f));
        }

        public Command GetCommand()
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
                ret = guardState.IsAutoGuard ? attack : guard;
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
}
