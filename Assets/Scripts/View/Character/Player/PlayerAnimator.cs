using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

[RequireComponent(typeof(SpringManager))]
public class PlayerAnimator : ShieldAnimator
{
    protected StateMachineTrigger trigger;

    private int defaultStateID = Animator.StringToHash("Base Layer.Stand.Idle");

    public TriggerJump jump { get; protected set; }
    public TriggerJump pitJump { get; protected set; }
    public TriggerJump landing { get; protected set; }
    public TriggerBrakeAndBackStep brakeAndBackStep { get; protected set; }

    public TriggerEx handle { get; protected set; }
    public TriggerEx putItem { get; protected set; }
    public TriggerEx getItem { get; protected set; }
    protected TriggerEx[] attack = new TriggerEx[4];
    public TriggerEx Attack(int index) => attack[index];
    public TriggerEx dropFloor { get; protected set; }
    public TriggerEx brake { get; protected set; }
    public TriggerEx fire { get; protected set; }
    public AnimatorBool rest { get; protected set; }
    public AnimatorBool handOn { get; protected set; }
    public AnimatorBool critical { get; protected set; }
    public AnimatorBool chargeUp { get; protected set; }
    public AnimatorBool coin { get; protected set; }
    public AnimatorBool fall { get; protected set; }
    public AnimatorFloat lifeRatio { get; protected set; }
    public AnimatorFloat jumpHeight { get; protected set; }
    public AnimatorFloat brakeOverRun { get; protected set; }
    public AnimatorFloat fallHeight { get; protected set; }
    public AnimatorFloat rSpeed { get; protected set; }

    private PlayerBodyCollider bodyCollider;
    private SpringManager springManager;

    protected override void Awake()
    {
        base.Awake();

        turnL = new TriggerEx(triggers, anim, "TurnL");
        turnR = new TriggerEx(triggers, anim, "TurnR");
        handle = new TriggerEx(triggers, anim, "Handle");
        putItem = new TriggerEx(triggers, anim, "PutItem");
        getItem = new TriggerEx(triggers, anim, "GetItem");
        attack[0] = new TriggerEx(triggers, anim, "Attack00", 5);
        attack[1] = new TriggerEx(triggers, anim, "Attack01", 5);
        attack[2] = new TriggerEx(triggers, anim, "Attack02", 5);
        attack[3] = new TriggerEx(triggers, anim, "Attack03", 5);
        dropFloor = new TriggerEx(triggers, anim, "DropFloor", 0);
        brake = new TriggerEx(triggers, anim, "Brake");
        fire = new TriggerEx(triggers, anim, "Fire");
        critical = new AnimatorBool(anim, "Critical");
        chargeUp = new AnimatorBool(anim, "ChargeUp");
        coin = new AnimatorBool(anim, "Coin");
        lifeRatio = new AnimatorFloat(anim, "LifeRatio");
        jumpHeight = new AnimatorFloat(anim, "JumpHeight");
        brakeOverRun = new AnimatorFloat(anim, "BrakeOverRun");
        fallHeight = new AnimatorFloat(anim, "FallHeight");
        rSpeed = new AnimatorFloat(anim, "RSpeed");

        springManager = GetComponent<SpringManager>();
    }

    protected void Start()
    {
        // StateMachineTrigger may have been prepared on Start()
        trigger = anim.GetBehaviour<StateMachineTrigger>();

        rest = new BoolRest(this);
        handOn = new BoolHandOn(this);

        bodyCollider = new PlayerBodyCollider(GetComponent<CapsuleCollider>());
        jump = new TriggerJump(this, jumpHeight, bodyCollider);
        pitJump = new TriggerJump(this, jumpHeight, bodyCollider, "PitJump");
        landing = new TriggerJump(this, jumpHeight, bodyCollider, "Landing");
        brakeAndBackStep = new TriggerBrakeAndBackStep(this, brakeOverRun, bodyCollider);
        fall = new BoolFall(this, fallHeight, bodyCollider);
    }

    public void ResetToIdle()
    {
        anim.Play(defaultStateID);
    }

    public override void Pause()
    {
        anim.enabled = false;
        springManager.Pause();
    }

    public override void Resume()
    {
        anim.enabled = true;
        springManager.Resume();
    }

    public abstract class TriggerUpdate : TriggerEx
    {
        protected IDisposable updateCollider = null;
        protected AnimatorFloat animatorFloat;
        protected PlayerBodyCollider bodyCollider;
        protected PlayerAnimator playerAnim;

        public TriggerUpdate(PlayerAnimator playerAnim, AnimatorFloat animatorFloat, PlayerBodyCollider bodyCollider, string varName, string fullPathStateName)
            : base(playerAnim.triggers, playerAnim.anim, varName)
        {
            this.playerAnim = playerAnim;
            this.animatorFloat = animatorFloat;
            this.bodyCollider = bodyCollider;

            // Disable the updating collider when exiting trigger entered state
            playerAnim.trigger.AddStateExit(Animator.StringToHash(fullPathStateName), _ =>
            {
                updateCollider?.Dispose();
                bodyCollider.ResetCollider();
            });
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
        public TriggerJump(PlayerAnimator playerAnim, AnimatorFloat jumpHeight, PlayerBodyCollider bodyCollider, string triggerName = "Jump")
            : base(playerAnim, jumpHeight, bodyCollider, triggerName, "Base Layer.Jump." + triggerName) { }
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
            int waitHash = Animator.StringToHash("Base Layer.Stand.Wait");
            int exhaustionHash = Animator.StringToHash("Base Layer.Stand.Exhaustion");

            playerAnim.trigger.AddStateEnter(waitHash, _ => Bool = true);
            playerAnim.trigger.AddStateEnter(exhaustionHash, _ => Bool = true);
        }
    }

    public class BoolHandOn : AnimatorBool
    {
        public BoolHandOn(PlayerAnimator playerAnim) : base(playerAnim.anim, "HandOn")
        {
            int handOnHash = Animator.StringToHash("Base Layer.Handle.HandOn");

            // Avoid hand on flag remaining after transition to another state.
            playerAnim.trigger.AddStateExit(handOnHash, _ => Bool = false);
        }
    }

    public class BoolFall : AnimatorBool
    {
        protected IDisposable updateCollider = null;
        protected AnimatorFloat animatorFloat;
        protected PlayerBodyCollider bodyCollider;
        protected PlayerAnimator playerAnim;

        public BoolFall(PlayerAnimator playerAnim, AnimatorFloat animatorFloat, PlayerBodyCollider bodyCollider)
            : base(playerAnim.anim, "Fall")
        {
            this.playerAnim = playerAnim;
            this.animatorFloat = animatorFloat;
            this.bodyCollider = bodyCollider;
        }

        public override bool Bool
        {
            get
            {
                return anim.GetBool(hashedVar);
            }
            set
            {
                if (value)
                {
                    updateCollider = playerAnim.UpdateAsObservable()
                        .Subscribe(_ => bodyCollider.IcedFallCollider(animatorFloat.Float))
                        .AddTo(playerAnim);
                }
                else
                {
                    updateCollider?.Dispose();
                    bodyCollider.ResetCollider();
                }

                anim.SetBool(hashedVar, value);
            }
        }
    }
}
