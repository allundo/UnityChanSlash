using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;
using UnityChan;

public class PlayerAnimator : ShieldAnimator
{
    public TriggerJump jump { get; protected set; }
    public TriggerBrakeAndBackStep brakeAndBackStep { get; protected set; }

    public TriggerEx turnL { get; protected set; }
    public TriggerEx turnR { get; protected set; }
    public TriggerEx handle { get; protected set; }
    public TriggerEx putItem { get; protected set; }
    public TriggerEx getItem { get; protected set; }
    public TriggerEx jab { get; protected set; }
    public TriggerEx straight { get; protected set; }
    public TriggerEx kick { get; protected set; }
    public TriggerEx dieEx { get; protected set; }
    public TriggerEx dropFloor { get; protected set; }
    public TriggerEx brake { get; protected set; }
    public AnimatorBool rest { get; protected set; }
    public AnimatorBool handOn { get; protected set; }
    public AnimatorBool cancel { get; protected set; }
    public AnimatorBool damage { get; protected set; }
    public AnimatorFloat jumpHeight { get; protected set; }
    public AnimatorFloat brakeOverRun { get; protected set; }
    public AnimatorFloat rSpeed { get; protected set; }

    private PlayerBodyCollider bodyCollider;
    private RandomWind randomWind;

    public override void Pause() { randomWind.isWindActive = false; anim.speed = 0.0f; }
    protected override void Start() { randomWind.isWindActive = true; anim.speed = 1.0f; }

    protected override void Awake()
    {
        base.Awake();

        turnL = new TriggerEx(anim, "TurnL");
        turnR = new TriggerEx(anim, "TurnR");
        handle = new TriggerEx(anim, "Handle");
        putItem = new TriggerEx(anim, "PutItem");
        getItem = new TriggerEx(anim, "GetItem");
        jab = new TriggerEx(anim, "Jab", 5);
        straight = new TriggerEx(anim, "Straight", 5);
        kick = new TriggerEx(anim, "Kick", 5);
        dieEx = new TriggerEx(anim, "Die", 0);
        dropFloor = new TriggerEx(anim, "DropFloor", 0);
        brake = new TriggerEx(anim, "Brake");
        handOn = new AnimatorBool(anim, "HandOn");
        cancel = new AnimatorBool(anim, "Cancel");
        jumpHeight = new AnimatorFloat(anim, "JumpHeight");
        brakeOverRun = new AnimatorFloat(anim, "BrakeOverRun");
        rSpeed = new AnimatorFloat(anim, "RSpeed");

        bodyCollider = new PlayerBodyCollider(GetComponent<CapsuleCollider>());
        randomWind = GetComponent<RandomWind>();

        rest = new BoolRest(this);

        jump = new TriggerJump(this, jumpHeight, bodyCollider);
        brakeAndBackStep = new TriggerBrakeAndBackStep(this, brakeOverRun, bodyCollider);
    }

    public abstract class TriggerUpdate : TriggerEx
    {
        protected IDisposable updateCollider = null;
        protected AnimatorFloat animatorFloat;
        protected PlayerBodyCollider bodyCollider;
        protected PlayerAnimator playerAnim;

        public TriggerUpdate(PlayerAnimator playerAnim, AnimatorFloat animatorFloat, PlayerBodyCollider bodyCollider, string varName, string fullPathStateName) : base(playerAnim.anim, varName)
        {
            this.playerAnim = playerAnim;
            this.animatorFloat = animatorFloat;
            this.bodyCollider = bodyCollider;

            // Disable the updateing collider when exiting trigger entered state
            playerAnim.StateExit
                .Where(x => x.StateInfo.fullPathHash == Animator.StringToHash(fullPathStateName))
                .Subscribe(_ =>
                {
                    updateCollider?.Dispose();
                    bodyCollider.ResetCollider();
                })
                .AddTo(playerAnim);
        }

        protected abstract void Update(float value);

        public override void Execute()
        {
            anim.SetTrigger(hashedVar);

            // Start updating collider every frame
            updateCollider = playerAnim.UpdateAsObservable()
                .Subscribe(_ => Update(animatorFloat.Float))
                .AddTo(playerAnim);
        }
    }

    public class TriggerJump : TriggerUpdate
    {
        public TriggerJump(PlayerAnimator playerAnim, AnimatorFloat jumpHeight, PlayerBodyCollider bodyCollider)
            : base(playerAnim, jumpHeight, bodyCollider, "Jump", "Base Layer.Jump") { }
        protected override void Update(float value) => bodyCollider.JumpCollider(value);
    }

    public class TriggerBrakeAndBackStep : TriggerUpdate
    {
        public TriggerBrakeAndBackStep(PlayerAnimator playerAnim, AnimatorFloat brakeOverRun, PlayerBodyCollider bodyCollider)
            : base(playerAnim, brakeOverRun, bodyCollider, "BrakeAndBackStep", "Base Layer.Move.BrakeAndBackStep") { }
        protected override void Update(float value) => bodyCollider.OverRunCollider(value);
    }

    public class BoolRest : AnimatorBool
    {
        public BoolRest(PlayerAnimator playerAnim) : base(playerAnim.anim, "Rest")
        {
            playerAnim.StateEnter
                .Where(x => x.StateInfo.fullPathHash == Animator.StringToHash("Base Layer.Stand.Rest"))
                .Subscribe(_ => Bool = true)
                .AddTo(playerAnim);
        }

    }
}
