using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class MobMatColorEffect : MatColorEffect
{
    public MobMatColorEffect(Transform targetTf, Transform exclude = null) : base(targetTf)
    {
        if (exclude == null) return;
        RetrieveMaterials(exclude).ForEach(mat => materials.Remove(mat));
    }

    public MobMatColorEffect(List<Material> materials, List<Material> excludes = null) : base(materials)
    {
        excludes?.ForEach(mat => materials.Remove(mat));
    }

    public void HealFlash(float duration)
    {
        Sequence flash = DOTween.Sequence();

        foreach (Material mat in materials)
        {
            flash.Join(
                DOTween.Sequence()
                    .Append(mat.DOColor(new Color(0.5f, 0.5f, 1f), duration * 0.5f))
                    .Append(mat.DOColor(Color.black, duration * 0.5f).SetEase(Ease.InQuad))
            );
        }

        PlayExclusive(flash);
    }
}