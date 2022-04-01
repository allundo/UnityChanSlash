using UnityEngine;
using DG.Tweening;

public class SkeletonWizEffect : UndeadEffect
{
    protected Tween teleportWipeTween;
    protected int propClipY;
    protected float clipY
    {
        get
        {
            return flashMaterials[0].GetFloat(propClipY);
        }
        set
        {
            flashMaterials.ForEach(mat => mat.SetFloat(propClipY, value));
        }
    }

    protected override void Awake()
    {
        base.Awake();
        propClipY = Shader.PropertyToID("_ClipY");
    }

    public void TeleportWipe(float duration)
    {
        teleportWipeTween = DOTween.Sequence()
            .AppendInterval(0.1f * duration)
            .Append(DOVirtual.Float(0f, 2.5f, 0.35f * duration, value => clipY = value).SetEase(Ease.InCubic))
            .AppendInterval(0.05f * duration)
            .SetLoops(2, LoopType.Yoyo)
            .SetUpdate(false)
            .Play();

        PlayFlash(DOTween.Sequence().Append(FadeOutTween(0.5f * duration)).Append(FadeInTween(0.5f * duration)).SetUpdate(false));
    }

    public void TeleportFX()
    {
        PlayBodyVFX(VFXType.Teleport, transform.position);
        PlayBodySnd(SNDType.Teleport, transform.position);
    }

    public void TeleportDestFX()
    {
        StopBodyVFX(VFXType.Teleport);
        PlayBodyVFX(VFXType.TeleportDest, transform.position);
        PlayBodySnd(SNDType.TeleportDest, transform.position);
    }

    public void OnTeleportEnd()
    {
        StopBodyVFX(VFXType.TeleportDest);
    }

    /// <summary>
    /// Stop teleport motion OnDie()
    /// </summary>
    protected override void StopAnimFX()
    {
        teleportWipeTween?.Complete();
    }
}
