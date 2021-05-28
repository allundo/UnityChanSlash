using UnityEngine;
using System;
using System.Collections.Generic;

public class UnityChanAnimeHandler : ShieldAnimator
{
    public TriggerEx jump { get; protected set; }
    public TriggerEx turnL { get; protected set; }
    public TriggerEx turnR { get; protected set; }
    public TriggerEx handle { get; protected set; }
    public TriggerEx attack { get; protected set; }
    public TriggerEx dieEx { get; protected set; }
    public AnimatorBool rest { get; protected set; }
    public AnimatorFloat jumpHeight { get; protected set; }
    public AnimatorFloat rSpeed { get; protected set; }

    protected override void Awake()
    {
        base.Awake();

        jump = new TriggerEx(anim, "Jump");
        turnL = new TriggerEx(anim, "TurnL");
        turnR = new TriggerEx(anim, "TurnR");
        handle = new TriggerEx(anim, "Handle");
        attack = new TriggerEx(anim, "Attack", 5);
        dieEx = new TriggerEx(anim, "Die", 0);
        rest = new AnimatorBool(anim, "Rest");
        jumpHeight = new AnimatorFloat(anim, "JumpHeight");
        rSpeed = new AnimatorFloat(anim, "RSpeed");
    }

    protected override Dictionary<string, AnimeState> GetStateNameMap()
    {
        var map = base.GetStateNameMap();

        map["Move.Locomotion"] = map["Move.WalkBack"] = map["Move.WalkL"] = map["Move.WalkR"]
            = map["Turn.TurnL"] = map["Turn.TurnR"]
            = map["Handle"]
            = map["Attack"]
            = map["Die"]
            = map["Stand.Idle"] = map["Stand.Rest"] = map["Guard"] = map["Shield"]
            = standardState;

        map["Jump"] = new JumpState(anim, new JumpCollider(bodyCollider));

        return map;
    }

    protected class JumpState : AnimeState
    {
        public JumpState(Animator anim, ColliderState colState) : base(anim, colState) { }

        public override void UpdateState()
        {
            colState.UpdateCollider(anim.GetFloat("JumpHeight"));
        }
    }

    protected class JumpCollider : ColliderState
    {
        public JumpCollider(CapsuleCollider col, float threshold = 0.001f) : base(col, threshold) { }

        public override void UpdateCollider(float jumpHeight)
        {
            if (jumpHeight < threshold)
            {
                ResetCollider();
                return;
            }

            col.height = orgColHeight - jumpHeight;
            float adjCenterY = orgColCenter.y + jumpHeight;
            col.center = new Vector3(0, adjCenterY, 0);
        }
    }
}
