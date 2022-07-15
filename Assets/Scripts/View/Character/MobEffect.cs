using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public interface IMobEffect : IBodyEffect
{
    void OnMelt();
    void OnIced(Vector3 pos);
    void OnIceCrash(Vector3 pos);

    /// <summary>
    /// Play body effect on heal
    /// </summary>
    /// <param name="healRatio">Normalized heal ratio to the life max</param>
    void OnHeal(float healRatio);

    void OnAppear();
    void OnHide();
}

public class MobEffect : MonoBehaviour, IMobEffect
{
    [SerializeField] protected AudioSource dieSound = null;

    [SerializeField] protected Transform excludeBody = null;

    protected DamageSndData sndData;

    /// <summary>
    /// Animation effects played by Animation clip. They need to stop on dying.
    /// </summary>
    protected AnimationFX animFX;

    protected ResourceFX resourceFX;
    protected MobMatColorEffect matColEffect;

    protected AudioSource SndDamage(AttackType type) => Util.Instantiate(sndData.Param((int)type).damage, transform);
    protected Dictionary<AttackType, AudioSource> damageSndSource = new Dictionary<AttackType, AudioSource>();
    protected void PlayDamage(AttackType type) => damageSndSource.LazyLoad(type, SndDamage).PlayEx();

    protected AudioSource SndCritical(AttackType type) => Util.Instantiate(sndData.Param((int)type).critical, transform);
    protected Dictionary<AttackType, AudioSource> criticalSndSource = new Dictionary<AttackType, AudioSource>();
    protected void PlayCritical(AttackType type) => criticalSndSource.LazyLoad(type, SndCritical).PlayEx();

    protected AudioSource SndGuard(AttackType type) => Util.Instantiate(sndData.Param((int)type).guard, transform);
    protected Dictionary<AttackType, AudioSource> guardSndSource = new Dictionary<AttackType, AudioSource>();
    protected void PlayGuard(AttackType type) => guardSndSource.LazyLoad(type, SndGuard).PlayEx();

    protected virtual void Awake()
    {
        matColEffect = new MobMatColorEffect(transform, excludeBody);
        sndData = Resources.Load<DamageSndData>("DataAssets/Sound/DamageSndData");
        animFX = GetComponent<AnimationFX>();
        resourceFX = new ResourceFX(transform);
    }

    public virtual void OnActive()
    {
        matColEffect.Activate();
    }

    public void Disappear(TweenCallback onComplete = null, float duration = 0.5f)
         => matColEffect.Inactivate(onComplete, duration);

    public virtual void OnHeal(float healRatio)
    {
        matColEffect.HealFlash(healRatio * 0.5f);
    }

    public void OnDie()
    {
        dieSound.PlayEx();
        resourceFX.StopVFX(VFXType.Iced);
        StopAllAnimation();
    }

    public void OnAppear()
    {
        matColEffect.FadeIn();
    }

    public void OnHide()
    {
        matColEffect.FadeOut();
    }

    public virtual void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None)
    {
        DamageSound(damageRatio, type);
        if (attr != AttackAttr.Ice) matColEffect.DamageFlash(damageRatio);
    }

    protected void DamageSound(float damageRatio, AttackType type = AttackType.None)
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

    public virtual void OnIced(Vector3 pos)
    {
        resourceFX.PlayVFX(VFXType.Iced, pos);
        matColEffect.Flash(new Color(0f, 0.5f, 0.5f, 1f), 0.1f);
    }

    public virtual void OnMelt()
    {
        resourceFX.StopVFX(VFXType.Iced);
        matColEffect.Flash(Color.black, 0.5f);
    }

    public virtual void OnIceCrash(Vector3 pos)
    {
        PlayCritical(AttackType.Ice);
        resourceFX.PlayVFX(VFXType.IceCrash, pos);
    }

    protected virtual void StopAllAnimation()
    {
        animFX?.StopVFX();
    }

    public virtual void OnDestroyByReactor()
    {
        matColEffect.KillAllTweens();
    }
}
