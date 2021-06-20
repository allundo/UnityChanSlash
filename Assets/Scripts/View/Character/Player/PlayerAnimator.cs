using UnityEngine;
using UniRx;
using System;
using UnityChan;

public class PlayerAnimator : ShieldAnimator
{
    public TriggerJump jump { get; protected set; }

    public TriggerEx turnL { get; protected set; }
    public TriggerEx turnR { get; protected set; }
    public TriggerEx handle { get; protected set; }
    public TriggerEx attack { get; protected set; }
    public TriggerEx straight { get; protected set; }
    public TriggerEx kick { get; protected set; }
    public TriggerEx dieEx { get; protected set; }
    public AnimatorBool rest { get; protected set; }
    public AnimatorBool handOn { get; protected set; }
    public AnimatorFloat jumpHeight { get; protected set; }
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
        attack = new TriggerEx(anim, "Attack", 5);
        straight = new TriggerEx(anim, "Straight", 5);
        kick = new TriggerEx(anim, "Kick", 5);
        dieEx = new TriggerEx(anim, "Die", 0);
        rest = new AnimatorBool(anim, "Rest");
        handOn = new AnimatorBool(anim, "HandOn");
        jumpHeight = new AnimatorFloat(anim, "JumpHeight");
        rSpeed = new AnimatorFloat(anim, "RSpeed");

        bodyCollider = new PlayerBodyCollider(GetComponent<CapsuleCollider>());
        randomWind = GetComponent<RandomWind>();

        jump = new TriggerJump(this, jumpHeight, bodyCollider);
    }

    public class TriggerJump : TriggerEx
    {
        protected IDisposable updateCollider = null;
        protected AnimatorFloat jumpHeight;
        protected PlayerBodyCollider bodyCollider;
        protected PlayerAnimator playerAnim;

        public TriggerJump(PlayerAnimator playerAnim, AnimatorFloat jumpHeight, PlayerBodyCollider bodyCollider) : base(playerAnim.anim, "Jump")
        {
            this.playerAnim = playerAnim;
            this.jumpHeight = jumpHeight;
            this.bodyCollider = bodyCollider;

            // Disable the updateing collider when exiting state "Jump"
            playerAnim.StateExit
                .Where(x => x.StateInfo.fullPathHash == hashedVar)
                .Subscribe(_ =>
                {
                    updateCollider?.Dispose();
                    bodyCollider.ResetCollider();
                })
                .AddTo(playerAnim);
        }

        public override void Execute()
        {
            anim.SetTrigger(hashedVar);

            // Start updating collider every frame
            updateCollider = Observable
                .IntervalFrame(1)
                .Subscribe(_ => bodyCollider.JumpCollider(jumpHeight.Float))
                .AddTo(playerAnim);
        }
    }
}
