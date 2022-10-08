using UnityEngine;

public class PlayerAnimFX : ShieldAnimFX
{
    [SerializeField] private AudioSource jumpSfx = null;
    [SerializeField] private AudioSource jumpLandingSfx = null;

    [SerializeField] private AudioSource brakeSfx = null;
    [SerializeField] private AudioSource brakeAndStepSfx = null;

    [SerializeField] private AudioSource stepSfx = null;

    public void OnJump() => fx.Play(jumpSfx);
    public void OnJumpLanding() => fx.Play(jumpLandingSfx);

    public void OnBrake() => fx.PlayPitch(brakeSfx, 0.9f, 1.1f);
    public void OnBrakeAndStep() => fx.Play(brakeAndStepSfx);

    public void OnStep(AnimationEvent evt)
    {
        if (evt.animatorClipInfo.weight > 0.5f) fx.PlayPitch(stepSfx, 0.8f, 1.2f);
    }

    public void OnRunningStep(AnimationEvent evt)
    {
        if (evt.animatorClipInfo.weight > 0.5f) fx.PlayPitch(stepSfx, 0.9f, 1.3f);
    }
}
