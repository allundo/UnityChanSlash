using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class MobEffect : BodyEffect
{
    [SerializeField] private AudioSource lifeMaxSound = null;

    protected AudioSource SndCritical(AttackType type) => Util.Instantiate(data.Param((int)type).critical, transform);
    protected Dictionary<AttackType, AudioSource> criticalSndSource = new Dictionary<AttackType, AudioSource>();
    protected void PlayCritical(AttackType type) => criticalSndSource.LazyLoad(type, SndCritical).PlayEx();

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

    protected override void DamageSound(float damageRatio, AttackType type = AttackType.None)
    {
        if (damageRatio < 0.000001f) return;

        if (damageRatio > 0.1f)
        {
            PlayCritical(type);
        }
        else
        {
            PlayDamage(type);
        }
    }
}
