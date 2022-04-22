using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class MatClipEffect : MaterialEffect
{
    protected Sequence wipeSeq;

    protected float clipY
    {
        get
        {
            return materials[0].GetFloat(propID);
        }
        set
        {
            materials.ForEach(mat => mat.SetFloat(propID, value));
        }
    }

    protected override string propName => "_ClipY";
    protected override void InitProperty(Material mat, int propID) => mat.SetFloat(propID, 0f);
    public MatClipEffect(Transform targetTf) : base(targetTf) { }
    public MatClipEffect(List<Material> materials) : base(materials) { }

    public void TeleportWipe(float duration)
    {
        wipeSeq?.Complete(true);

        wipeSeq = DOTween.Sequence()
            .AppendInterval(0.1f * duration)
            .Append(DOVirtual.Float(0f, 2.5f, 0.35f * duration, value => clipY = value).SetEase(Ease.InCubic))
            .AppendInterval(0.05f * duration)
            .SetLoops(2, LoopType.Yoyo)
            .SetUpdate(false)
            .Play();
    }

    public override void InitEffects()
    {
        wipeSeq?.Complete(true);
    }

    public override void KillAllTweens()
    {
        wipeSeq?.Complete(true);
    }
}