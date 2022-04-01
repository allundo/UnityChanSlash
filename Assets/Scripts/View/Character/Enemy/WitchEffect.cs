using UnityEngine;
using DG.Tweening;

public class WitchEffect : GhostEffect
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
            .Append(DOVirtual.Float(0f, 2.5f, 0.4f * duration, value => clipY = value).SetEase(Ease.InCubic))
            .SetLoops(2, LoopType.Yoyo)
            .SetUpdate(false)
            .Play();
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
    /// Stop charging trail and target attack trail OnDie()
    /// </summary>
    protected override void StopAnimFX()
    {
        OnAttackEnd();
        (animFX as WitchAnimFX).OnTrailEnd();
        teleportWipeTween?.Complete();
    }
}
