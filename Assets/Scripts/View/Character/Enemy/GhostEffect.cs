using UnityEngine;

public class GhostEffect : EnemyEffect
{
    [SerializeField] protected ParticleSystem emitVfx = default;
    [SerializeField] protected AudioSource attackSnd = default;

    protected MatVertexEffect matVertEffect;

    protected override void Awake()
    {
        base.Awake();
        matVertEffect = new MatVertexEffect(transform);
    }

    protected virtual void FixedUpdate() => matVertEffect.Update();

    public void OnAttackStart()
    {
        emitVfx?.Play();
        attackSnd.PlayEx();
        matVertEffect.trailTarget = 1.8f;
    }

    public void OnAttackEnd()
    {
        emitVfx?.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        matVertEffect.trailTarget = 0f;
    }

    /// <summary>
    /// Stop charging trail OnDie()
    /// </summary>
    protected override void StopAllAnimation()
    {
        base.StopAllAnimation();
        OnAttackEnd();
    }

    public override void OnActive()
    {
        matColEffect.Activate();
        matVertEffect.InitEffects();
    }
}
