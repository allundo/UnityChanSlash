using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class MobEffect : BodyEffect
{
    protected AudioSource SndCritical(AttackType type) => Util.Instantiate(sndData.Param((int)type).critical, transform);
    protected Dictionary<AttackType, AudioSource> criticalSndSource = new Dictionary<AttackType, AudioSource>();
    protected void PlayCritical(AttackType type) => criticalSndSource.LazyLoad(type, SndCritical).PlayEx();

    protected AudioSource SndGuard(AttackType type) => Util.Instantiate(sndData.Param((int)type).guard, transform);
    protected Dictionary<AttackType, AudioSource> guardSndSource = new Dictionary<AttackType, AudioSource>();
    protected void PlayGuard(AttackType type) => guardSndSource.LazyLoad(type, SndGuard).PlayEx();

    public override void OnHeal(float healRatio)
    {
        HealFlash(healRatio * 0.5f);
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
        if (damageRatio < 0.000001f)
        {
            PlayGuard(type);
            return;
        }

        if (damageRatio <= 0.1f)
        {
            PlayDamage(type);
            return;
        }

        PlayCritical(type);
    }

    public override void OnIceCrash(Vector3 pos)
    {
        PlayCritical(AttackType.Ice);
        PlayBodyVFX(VFXType.IceCrash, pos);
    }
}
