using System;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class ItemInfo : ICloneable
{
    protected ItemSource itemSource;

    public int numOfItem
    {
        get
        {
            return onNumOfItemChange.Value;
        }
        protected set
        {
            onNumOfItemChange.Value = value;
        }
    }

    public IReactiveProperty<int> onNumOfItemChange;
    public IReadOnlyReactiveProperty<int> OnNumOfItemChange => onNumOfItemChange;

    public bool Merge(ItemInfo itemInfo)
    {
        numOfItem += itemInfo.Subtraction();
        return true;
    }

    public int Subtraction(int numOfItem = 0)
    {
        numOfItem = numOfItem == 0 ? this.numOfItem : Mathf.Min(this.numOfItem, numOfItem);
        this.numOfItem -= numOfItem;
        return numOfItem;
    }

    public ItemType type { get; protected set; }

    public Material material => itemSource.material;

    /// <summary>
    /// Item use command duration
    /// </summary>
    public float duration => itemSource.duration;

    public string name => itemSource.name;
    public string description => itemSource.Description;
    public int Price => itemSource.unitPrice * numOfItem;

    protected AudioSource sfx = null;
    protected ParticleSystem vfx = null;

    protected ItemAction itemAction;
    public ItemAttr attr => itemAction.attr;

    public ItemInfo(ItemSource itemSource, ItemType type, ItemAction itemAction, int numOfItem = 1)
    {
        this.onNumOfItemChange = new ReactiveProperty<int>(numOfItem);

        this.type = type;
        this.itemAction = itemAction;
        this.itemSource = itemSource;
    }

    public object Clone() => Clone(this.numOfItem);

    public object Clone(int numOfItem)
        => new ItemInfo(itemSource, type, itemAction, numOfItem);

    protected virtual void FXStart(Transform user)
    {
        if (itemSource.sfx != null)
        {
            if (sfx == null) sfx = Util.Instantiate(itemSource.sfx);
            sfx.transform.position = user.position;
            sfx.transform.SetParent(user);
            sfx.PlayEx();
        }

        if (itemSource.vfx != null)
        {
            if (vfx == null) vfx = Util.Instantiate(itemSource.vfx);
            vfx.transform.position = user.position;
            vfx.transform.SetParent(user);
            vfx?.Play();
        }
    }

    public virtual Tween EffectSequence(PlayerCommandTarget target)
    {
        return ItemUse(target) ? DOTweenTimer(0f, () => FXStart(target.transform)) : null;
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
