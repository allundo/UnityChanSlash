using System;
using UnityEngine;
using UniRx;
using DG.Tweening;

public abstract class ItemInfo : ICloneable
{
    protected int numOfItem
    {
        get
        {
            return onNumOfItemChange.Value;
        }
        set
        {
            onNumOfItemChange.Value = value;
        }
    }

    public IReactiveProperty<int> onNumOfItemChange;
    public IReadOnlyReactiveProperty<int> OnNumOfItemChange => onNumOfItemChange;

    public Material material { get; protected set; }
    public float duration { get; protected set; }

    protected AudioSource sfx;
    protected ParticleSystem vfx;


    public ItemInfo(ItemSource itemSource, int numOfItem = 1)
        : this(itemSource.material, numOfItem, Util.Instantiate(itemSource.vfx), Util.Instantiate(itemSource.sfx), itemSource.duration)
    { }

    public ItemInfo(Material material, int numOfItem = 1, ParticleSystem vfx = null, AudioSource sfx = null, float duration = 0.2f)
    {
        this.onNumOfItemChange = new ReactiveProperty<int>(numOfItem);

        this.material = material;
        this.vfx = vfx;
        this.sfx = sfx;

        this.duration = duration;
    }

    public object Clone() => Clone(numOfItem);

    public abstract object Clone(int numOfItem);

    protected virtual void FXStart(Vector3 position)
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

    /// <summary>
    /// Item effect.
    /// </summary>
    /// <param name="target"></param>
    /// <returns>The number of item consumption.</returns>
    protected abstract int Action(PlayerCommandTarget target);

    public virtual Tween EffectSequence(PlayerCommandTarget target)
    {
        return ItemUse(target) ? DOTweenTimer(0f, () => FXStart(target.react.position)) : null;
    }

    protected bool ItemUse(PlayerCommandTarget target)
    {
        int useCount = Action(target);
        numOfItem -= useCount;
        return useCount > 0;
    }

    protected Tween DOTweenTimer(float dueTimeSec, TweenCallback callback, bool ignoreTimeScale = false)
        => DOVirtual.DelayedCall(dueTimeSec, callback, ignoreTimeScale);
}