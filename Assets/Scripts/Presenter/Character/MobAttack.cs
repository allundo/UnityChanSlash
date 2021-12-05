using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class MobAttack : Attack
{
    [SerializeField] protected AudioSource swingSound = default;
    [SerializeField] protected ParticleSystem vfx = default;

    [SerializeField] protected int startFrame = 0;
    [SerializeField] protected int finishFrame = 0;
    [SerializeField] protected int speed = 1;
    [SerializeField] protected float minPitch = 0.7f;
    [SerializeField] protected float maxPitch = 1.3f;
    [SerializeField] protected int frameRate = 30;

    protected virtual float Pitch => Random.Range(minPitch, maxPitch);

    private float FrameToSec(int frame)
    {
        return (float)frame / (float)frameRate / (float)speed;
    }

    protected virtual void OnAttackStart()
    {
        if (swingSound != null)
        {
            swingSound.pitch = Pitch;
            swingSound.Play();
        }
        vfx?.Play();
    }

    protected virtual void OnAttackFinished() { }

    public override Tween AttackSequence(float attackDuration)
    {
        return DOTween.Sequence()
            .AppendCallback(OnAttackStart)
            .Join(DOVirtual.DelayedCall(FrameToSec(startFrame), OnHitStart))
            .Join(DOVirtual.DelayedCall(FrameToSec(finishFrame), OnHitFinished))
            .Join(DOVirtual.DelayedCall(attackDuration, OnAttackFinished))
            .SetUpdate(false);
    }
}
