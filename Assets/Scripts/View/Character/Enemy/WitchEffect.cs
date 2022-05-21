using UnityEngine;

public class WitchEffect : GhostEffect
{
    [SerializeField] private AudioSource summonSnd = default;
    [SerializeField] private ParticleSystem summonVFX = default;
    protected MatClipEffect matClipEffect;

    protected override void Awake()
    {
        base.Awake();
        matClipEffect = new MatClipEffect(matColEffect.CopyMaterials());
    }

    public void OnSummonStart()
    {
        summonSnd.PlayEx();
        summonVFX?.Play();
    }

    public void TeleportWipe(float duration)
    {
        matClipEffect.TeleportWipe(duration);
    }

    public void TeleportFX()
    {
        resourceFX.PlayVFX(VFXType.Teleport, transform.position);
        resourceFX.PlaySnd(SNDType.Teleport, transform.position);
    }

    public void TeleportDestFX()
    {
        resourceFX.StopVFX(VFXType.Teleport);
        base.SummonFX();
    }

    /// <summary>
    /// Stop charging trail and target attack trail OnDie().
    /// </summary>
    protected override void StopAllAnimation()
    {
        base.StopAllAnimation();
        matClipEffect.InitEffects();
    }
}
