using UnityEngine;
using System;
using System.Collections.Generic;

public class UnityChanAnimeHandler : AnimeHandler
{

    protected override AnimeStateTypes GetAnimeStateTypes()
    {
        AnimeStateTypes types = base.GetAnimeStateTypes();
        types.jumpState = new JumpState(anim, new JumpCollider(col));

        return types;
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