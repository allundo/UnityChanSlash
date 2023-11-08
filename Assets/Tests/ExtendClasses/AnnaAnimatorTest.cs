using UnityEngine;
using DG.Tweening;

public class AnnaAnimatorTest : AnnaAnimator
{
    public Tween speedTween { get; protected set; }
    public Tween speedLRTween { get; protected set; }

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

    protected void SetSpeedLR(Tween speedLRTween)
    {
        if (this.speedLRTween != speedLRTween) this.speedLRTween?.Kill();
        this.speedLRTween = speedLRTween?.Play();
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

    public void StartMovingLR(float targetSpd)
    {
        speedCommand.Bool = true;
        SetSpeedLR(DOTween.To(() => speedLR.Float, value => speedLR.Float = value, targetSpd, 0.5f));
    }

    public void EndMovingLR()
    {
        speedCommand.Bool = false;
        SetSpeedLR(DOTween.To(() => speedLR.Float, value => speedLR.Float = value, 0f, 0.25f));
    }

    public void StartJump(float targetSpd = 8f)
    {
        SetSpeed(null);
        speed.Float = targetSpd;
        jump.Bool = true;
    }

    public void EndJump()
    {
        jump.Bool = false;
        speed.Float = 0f;
    }
}
