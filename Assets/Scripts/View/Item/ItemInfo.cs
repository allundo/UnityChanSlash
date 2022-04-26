using System;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class ItemInfo : ICloneable
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

    /// <summary>
    /// Item use command duration
    /// </summary>
    public float duration { get; protected set; }

    protected AudioSource sfx;
    protected ParticleSystem vfx;

    protected ItemAction itemAction;
    public ItemAttr attr => itemAction.attr;

    public ItemInfo(ItemSource itemSource, ItemAction itemAction, int numOfItem = 1)
        : this(itemSource.material, itemAction, numOfItem, Util.Instantiate(itemSource.vfx), Util.Instantiate(itemSource.sfx), itemSource.duration)
    { }

    public ItemInfo(Material material, ItemAction itemAction, int numOfItem = 1, ParticleSystem vfx = null, AudioSource sfx = null, float duration = 0.2f)
    {
        this.onNumOfItemChange = new ReactiveProperty<int>(numOfItem);

        this.itemAction = itemAction;

        this.material = material;
        this.vfx = vfx;
        this.sfx = sfx;

        this.duration = duration;
    }

    public object Clone() => Clone(numOfItem);

    public object Clone(int numOfItem)
        => new ItemInfo(material, itemAction, numOfItem, vfx, sfx);

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

    public virtual Tween EffectSequence(PlayerCommandTarget target)
    {
        return ItemUse(target) ? DOTweenTimer(0f, () => FXStart(target.react.position)) : null;
    }

    protected bool ItemUse(PlayerCommandTarget target)
    {
        int useCount = itemAction.Action(target);
        numOfItem -= useCount;
        return useCount > 0;
    }

    protected Tween DOTweenTimer(float dueTimeSec, TweenCallback callback, bool ignoreTimeScale = false)
        => DOVirtual.DelayedCall(dueTimeSec, callback, ignoreTimeScale);
}
