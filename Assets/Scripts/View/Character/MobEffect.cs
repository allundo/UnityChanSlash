using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public interface IMobEffect : IBodyEffect
{
    void OnMelt();
    void OnIced(Vector3 pos, bool isPaused = true);
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

    protected MobAnimator anim;

    /// <summary>
    /// Animation effects played by Animation clip. They need to stop on dying.
    /// </summary>
    protected AnimationFX animFX;

    protected ResourceFX resourceFX;
    protected MobMatColorEffect matColEffect;

    protected virtual AudioSource SndInstance(AudioSource src) => Util.Instantiate(src, transform);
    protected AudioSource SndDamageInstance(AttackType type) => SndInstance(sndData.Param((int)type).damage);
    protected Dictionary<AttackType, AudioSource> damageSndSource = new Dictionary<AttackType, AudioSource>();
    protected virtual AudioSource SndDamage(AttackType type, IDirection dir = null) => damageSndSource.LazyLoad(type, SndDamageInstance);

    protected AudioSource SndCriticalInstance(AttackType type) => SndInstance(sndData.Param((int)type).critical);
    protected Dictionary<AttackType, AudioSource> criticalSndSource = new Dictionary<AttackType, AudioSource>();
    protected virtual AudioSource SndCritical(AttackType type, IDirection dir = null) => criticalSndSource.LazyLoad(type, SndCriticalInstance);

    protected AudioSource SndGuardInstance(AttackType type) => SndInstance(sndData.Param((int)type).guard);
    protected Dictionary<AttackType, AudioSource> guardSndSource = new Dictionary<AttackType, AudioSource>();
    protected AudioSource SndGuard(AttackType type) => guardSndSource.LazyLoad(type, SndGuardInstance);

    protected virtual void Awake()
    {
        anim = GetComponent<MobAnimator>();
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

    public virtual void OnDamage(float damageRatio, AttackType type = AttackType.None, AttackAttr attr = AttackAttr.None, IDirection dir = null)
    {
        DamageSound(damageRatio, type, dir);
        if (attr != AttackAttr.Ice) matColEffect.DamageFlash(damageRatio);
        if (attr == AttackAttr.Light) OnHitLaser();
    }

    protected virtual void DamageSound(float damageRatio, AttackType type = AttackType.None, IDirection dir = null)
    {
        if (damageRatio < 0.000001f)
        {
            SndGuard(type).PlayEx();
            return;
        }

        if (damageRatio <= 0.2f)
        {
            SndDamage(type, dir).PlayEx();
            return;
        }

        SndCritical(type, dir).PlayEx();
    }

    public void OnIced(Vector3 pos, bool isPaused = true)
    {
        if (isPaused) anim.Pause();
        resourceFX.PlayVFX(VFXType.Iced, pos);
        matColEffect.Flash(new Color(0f, 0.5f, 0.5f, 1f), 0.1f);
    }

    public virtual void OnMelt()
    {
        anim.Resume();
        resourceFX.StopVFX(VFXType.Iced);
        matColEffect.Flash(Color.black, 0.5f);
    }

    public virtual void OnIceCrash(Vector3 pos)
    {
        SndCritical(AttackType.Ice).PlayEx();
        resourceFX.PlayVFX(VFXType.IceCrash, pos);
    }

    protected Tween fxTimer;
    protected void OnHitLaser(float duration = 0.8f)
    {
        if (fxTimer != null && fxTimer.IsActive())
        {
            fxTimer.Kill();
        }
        else
        {
            resourceFX.PlayVFX(VFXType.HitLaser);
        }

        resourceFX.PlaySnd(SNDType.HitLaser);
        fxTimer = DOVirtual.DelayedCall(duration, () => resourceFX.StopVFX(VFXType.HitLaser), false).Play();
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
