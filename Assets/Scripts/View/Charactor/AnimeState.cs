using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimeState
{
    protected Animator anim;
    protected ColliderState colState;

    protected float threshold = 0.0001f;

    public virtual bool IsShieldReady => false;

    public AnimeState(Animator anim, ColliderState colState)
    {
        this.anim = anim;
        this.colState = colState;
    }

    public virtual void UpdateState()
    {
        colState.UpdateCollider();
    }

    public virtual void ResetCollider()
    {
        colState.ResetCollider();
    }
}

