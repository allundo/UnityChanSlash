using UnityEngine;

public class AnimeState
{
    protected Animator anim;

    public AnimeState(Animator anim)
    {
        this.anim = anim;
    }

    public virtual void UpdateState() { }
}

