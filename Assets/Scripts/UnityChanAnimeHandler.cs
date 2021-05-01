using UnityEngine;
using System;
using System.Collections.Generic;

public class UnityChanAnimeHandler : AnimeHandler
{
    [SerializeField] protected Collider enemyDetector = default;
    protected override void Start()
    {
        base.Start();
        enemyDetector.enabled = false;
    }

    protected override Dictionary<string, AnimeState> GetStateNameMap()
    {
        var map = base.GetStateNameMap();

        map["Move.Locomotion"] = map["Move.WalkBack"] = map["Move.WalkL"] = map["Move.WalkR"]
            = map["Turn.TurnL"] = map["Turn.TurnR"]
            = map["Handle"]
            = map["Die"]
            = standardState;

        map["Jump"] = new JumpState(anim, new JumpCollider(col));

        map["Stand.Idle"] = map["Stand.Rest"] = map["Guard"] = map["Shield"]
            = new GuardableState(anim, new ColliderState(col), enemyDetector);

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

    protected class GuardableState : AnimeState
    {
        protected Collider enemyDetector;

        public GuardableState(Animator anim, ColliderState colState, Collider enemyDetector) : base(anim, colState)
        {
            this.enemyDetector = enemyDetector;
        }

        public override void UpdateState()
        {
            enemyDetector.enabled = true;
        }

        public override void ResetCollider()
        {
            base.ResetCollider();
            enemyDetector.enabled = false;
        }
    }
}
