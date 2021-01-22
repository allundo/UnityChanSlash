using UnityEngine;
using System;
using System.Collections.Generic;

public class UnityChanAnimeHandler : AnimeHandler
{
    protected override Dictionary<string, AnimeState> GetStateNameMap()
    {
        var map = base.GetStateNameMap();

        map["Move.Locomotion"] = map["Move.WalkBack"] = map["Move.WalkL"] = map["Move.WalkR"]
            = map["Turn.TurnL"] = map["Turn.TurnR"]
            = map["Rest"]
            = map["Handle"]
            = map["Die"]
            = standardState;

        map["Jump"] = new JumpState(anim, new JumpCollider(col));

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