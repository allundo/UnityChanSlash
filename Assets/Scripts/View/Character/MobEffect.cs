using UnityEngine;
using DG.Tweening;

public class MobEffect : BodyEffect
{
    [SerializeField] private AudioSource criticalSound = null;
    [SerializeField] private AudioSource lifeMaxSound = null;

    public override void OnHeal(float healRatio)
    {
        HealFlash(healRatio * 0.5f);
    }

    public override void OnLifeMax()
    {
        lifeMaxSound.PlayEx();
    }

    protected void HealFlash(float duration)
    {
        Sequence flash = DOTween.Sequence();

        foreach (Material mat in flashMaterials)
        {
            flash.Join(
                DOTween.Sequence()
                    .Append(mat.DOColor(new Color(0.5f, 0.5f, 1f), duration * 0.5f))
                    .Append(mat.DOColor(Color.black, duration * 0.5f).SetEase(Ease.InQuad))
            );
        }

        PlayFlash(flash);
    }

    protected override void DamageSound(float damageRatio)
    {
        if (damageRatio < 0.000001f) return;

        (damageRatio > 0.1f ? criticalSound : damageSound).PlayEx();
    }
}
