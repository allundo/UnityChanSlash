using UnityEngine;
using DG.Tweening;

public class AnimatorTest : ShieldAnimator
{
    public AnimatorFloat speedLR { get; protected set; }
    public AnimatorBool spdCmd { get; protected set; }
    public AnimatorTrigger attack { get; protected set; }
    public AnimatorTrigger interruption { get; protected set; }
    public AnimatorTrigger next { get; protected set; }
    public AnimatorBool run { get; protected set; }
    public AnimatorTrigger interruptionAny { get; protected set; }
    public AnimatorTrigger slash { get; protected set; }

    protected Tween speedTween;

    protected override void Awake()
    {
        anim = GetComponent<Animator>();

        speed = new AnimatorFloat(anim, "Speed");
        speedLR = new AnimatorFloat(anim, "SpeedLR");
        spdCmd = new AnimatorBool(anim, "SpeedCommand");
        attack = new ShieldAnimator.TriggerEx(triggers, anim, "Attack");
        interruption = new ShieldAnimator.TriggerEx(triggers, anim, "Interruption");
        interruptionAny = new ShieldAnimator.TriggerEx(triggers, anim, "InterruptionAny");
        slash = new ShieldAnimator.TriggerEx(triggers, anim, "Slash");
        next = new ShieldAnimator.TriggerEx(triggers, anim, "Next");
        run = new AnimatorBool(anim, "Run");
    }

    public bool IsCurrentState(string stateName)
    {
        string fullPathName = "Base Layer." + stateName;
        Debug.Log("check: " + fullPathName);
        return anim.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash(fullPathName);
    }

    public bool IsCurrentTransition(string from, string to)
        => anim.GetAnimatorTransitionInfo(0).IsName($"{from} -> {to}");

    public void StartMoving(float target)
    {
        spdCmd.Bool = true;
        speedTween?.Kill();
        speedTween = DOTween.To(() => speed.Float, value => speed.Float = value, target, 0.5f).Play();
    }

    public void EndMoving()
    {
        spdCmd.Bool = false;
        speedTween?.Kill();
        speedTween = DOTween.To(() => speed.Float, value => speed.Float = value, 0f, 0.25f).Play();
    }
}
