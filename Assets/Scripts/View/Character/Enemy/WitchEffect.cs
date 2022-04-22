public class WitchEffect : GhostEffect
{
    protected MatClipEffect matClipEffect;

    protected override void Awake()
    {
        base.Awake();
        matClipEffect = new MatClipEffect(matColEffect.CopyMaterials());
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
        resourceFX.PlayVFX(VFXType.TeleportDest, transform.position);
        resourceFX.PlaySnd(SNDType.TeleportDest, transform.position);
    }

    public void OnTeleportEnd()
    {
        resourceFX.StopVFX(VFXType.TeleportDest);
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
