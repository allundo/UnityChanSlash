using UnityEngine;
using DG.Tweening;

public class AnnaAnimatorTest : AnnaAnimator
{
    public Tween speedTween { get; protected set; }

    public bool IsCurrentState(string stateName)
    {
        string fullPathName = "Base Layer." + stateName;
        return anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash(fullPathName);
    }

    public bool IsCurrentTransition(string from, string to)
        => anim.GetAnimatorTransitionInfo(0).IsName($"{from} -> {to}");

    protected void SetSpeed(Tween speedTween)
    {
        if (this.speedTween != speedTween) this.speedTween?.Kill();
        this.speedTween = speedTween?.Play();
    }

    public void StartMoving(float targetSpd = 4f)
    {
        speedCommand.Bool = true;
        SetSpeed(DOTween.To(() => speed.Float, value => speed.Float = value, targetSpd, 0.5f));
    }

    public void EndMoving()
    {
        speedCommand.Bool = false;
        SetSpeed(DOTween.To(() => speed.Float, value => speed.Float = value, 0f, 0.25f));
    }
}
