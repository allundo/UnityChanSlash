using UnityEngine;
using DG.Tweening;

public class WitchEffect : GhostEffect
{
    [SerializeField] private AudioSource summonSnd = default;
    [SerializeField] private ParticleSystem summonVFX = default;
    [SerializeField] private Transform hatTf = default;

    protected MatClipEffect matClipEffect;
    protected Tween lightGenerateTimer;

    protected override void Awake()
    {
        base.Awake();
        matClipEffect = new MatClipEffect(matColEffect.CopyMaterials());

        // Spawn light periodically from hat position.
        lightGenerateTimer = DOVirtual.DelayedCall(0.3f, () => GameManager.Instance.DistributeLight(hatTf.position, 0.2f), false)
            .SetLoops(-1, LoopType.Restart)
            .AsReusable(gameObject);
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

    public void OnResurrection()
    {
        lightGenerateTimer.Restart();
    }

    public override void OnActive(float duration)
    {
        lightGenerateTimer.Restart();
        matColEffect.Activate(duration);
    }

    /// <summary>
    /// Stop charging trail and target attack trail OnDie().
    /// </summary>
    protected override void StopAllAnimation()
    {
        base.StopAllAnimation();
        matClipEffect.InitEffects();
        lightGenerateTimer.Pause();
    }
}
