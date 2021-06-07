using UnityEngine;
using System;
using System.Collections.Generic;

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
    public AnimatorFloat jumpHeight { get; protected set; }
    public AnimatorFloat rSpeed { get; protected set; }

    private PlayerBodyCollider bodyCollider;

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
        jumpHeight = new AnimatorFloat(anim, "JumpHeight");
        rSpeed = new AnimatorFloat(anim, "RSpeed");

        bodyCollider = new PlayerBodyCollider(GetComponent<CapsuleCollider>());

        jump = new TriggerJump(anim, jumpHeight, bodyCollider);
    }

    public class TriggerJump : TriggerEx
    {
        PlayerBodyCollider bodyCollider;
        AnimatorFloat jumpHeight;

        public TriggerJump(Animator anim, AnimatorFloat jumpHeight, PlayerBodyCollider bodyCollider) : base(anim, "Jump")
        {
            this.bodyCollider = bodyCollider;
            this.jumpHeight = jumpHeight;
        }

        /// <summary>
        /// Fires 'Jump' trigger with updating BodyCollider shape <br>
        /// The shape changes according to 'JumpHeight' Animator paramator<br>
        /// </summary>
        /// <param name="duration">Due time of 'JumpHeight' observation and update collider</param>
        /// <returns>Disposable of polling observer</returns>
        public IDisposable Fire(float duration)
        {
            return FireWithPolling(
                duration,
                _ => bodyCollider.JumpCollider(jumpHeight.Float),
                () => bodyCollider.ResetCollider()
            );
        }
    }
}
