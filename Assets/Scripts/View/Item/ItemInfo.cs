using System;
using UnityEngine;
using DG.Tweening;

public abstract class ItemInfo : ICloneable
{
    public int numOfItem { get; protected set; }

    public Material material { get; protected set; }
    public float duration { get; protected set; }

    protected AudioSource sfx;
    protected ParticleSystem vfx;

    public ItemInfo(Material material, int numOfItem = 1, ParticleSystem vfx = null, AudioSource sfx = null, float duration = 0.2f)
    {
        this.numOfItem = numOfItem;

        this.material = material;
        this.vfx = vfx;
        this.sfx = sfx;

        this.duration = duration;
    }

    public object Clone() => Clone(numOfItem);

    public abstract object Clone(int numOfItem);

    public int UseItem()
    {
        return --numOfItem;
    }

    protected virtual void OnFXStart(Vector3 position)
    {
        if (sfx != null)
        {
            sfx.transform.position = position;
            sfx.Play();
        }

        if (vfx != null)
        {
            vfx.transform.position = position;
            vfx.Play();
        }
    }

    protected virtual void OnAction(MobReactor react, MobAnimator anim) { }

    public virtual Tween EffectSequence(CommandTarget target)
    {
        return DOTween.Sequence()
            .Join(DOTweenTimer(0f, () => OnFXStart(target.transform.position)))
            .Join(DOTweenTimer(0f, () => OnAction(target.react, target.anim)))
            .SetUpdate(false);

    }

    protected Tween DOTweenTimer(float dueTimeSec, TweenCallback callback, bool ignoreTimeScale = false)
        => DOVirtual.DelayedCall(dueTimeSec, callback, ignoreTimeScale);
}