using UnityEngine;
using UniRx;
using System.Linq;
using System;
using System.Collections.Generic;

public interface IEquipmentStyle
{
    IEquipmentStyle Equip(int index, ItemIcon itemIcon);
    AttackButtonsHandler LoadAttackButtonsHandler(Transform attackInputUI);
    InputRegion LoadInputRegion(Transform fightCircle);
    FightStyleHandler LoadFightStyle(Transform player);
    RuntimeAnimatorController animatorController { get; }
}

public class EquipItemsHandler : IItemIndexHandler
{
    public int MAX_ITEMS { get; private set; }
    protected IReactiveProperty<ItemIcon> equipR = new ReactiveProperty<ItemIcon>(null);
    protected IReactiveProperty<ItemIcon> equipL = new ReactiveProperty<ItemIcon>(null);
    protected IReactiveProperty<ItemIcon> equipBody = new ReactiveProperty<ItemIcon>(null);
    protected IReactiveProperty<ItemIcon>[] equips;

    public IObservable<ItemInfo> EquipR => equipR.Select(icon => icon?.itemInfo);
    public IObservable<ItemInfo> EquipL => equipL.Select(icon => icon?.itemInfo);
    public IObservable<ItemInfo> EquipBody => equipBody.Select(icon => icon?.itemInfo);

    private IReactiveProperty<IEquipmentStyle> currentEquipments = new ReactiveProperty<IEquipmentStyle>();
    public IObservable<IEquipmentStyle> CurrentEquipments => currentEquipments;

    protected KnuckleKnuckle knuckleKnuckle = null;
    protected SwordKnuckle swordKnuckle = null;
    protected KnuckleShield knuckleShield = null;
    protected SwordShield swordShield = null;

    protected ResourceLoader resourceLoader;

    protected EquipmentSource GetEquipmentSource(ItemIcon itemIcon)
    {
        return itemIcon != null ? resourceLoader.GetEquipmentSource(itemIcon.itemInfo.type) : null;
    }

    private bool tweenMove = false;

    protected void SetEquip(int index, ItemIcon itemIcon)
    {
        equips[index].Value = itemIcon;

        itemEmptyCheck[index]?.Dispose();

        if (itemIcon != null)
        {
            if (tweenMove) itemIcon.MoveExclusive(UIPos(index));

            UpdateItemNum(itemIcon.SetIndex(index));

            itemEmptyCheck[index] = itemIcon.OnItemEmpty
                .Subscribe(_ => RemoveItem(GetEquip(index)))
                .AddTo(itemIcon);
        }
        else
        {
            panels[index].SetItemNum(0);
            itemEmptyCheck[index] = null;
        }
    }

    protected void SetEquipR(ItemIcon itemIcon) => SetEquip(2, itemIcon);
    protected void SetEquipL(ItemIcon itemIcon) => SetEquip(0, itemIcon);

    protected ItemIcon GetEquip(int index) => equips[index].Value;
    protected ItemIcon GetEquipR() => equipR.Value;
    protected ItemIcon GetEquipL() => equipL.Value;

    private IDisposable[] itemEmptyCheck;

    private ItemPanel[] panels;

    private Vector2 unit;
    private Vector2 offsetCenter;
    public Vector2 inventoryOrigin { get; protected set; }
    private Vector2 offsetOrigin;
    private RectTransform inventoryRT;
    private Transform inventoryTf;

    private Vector2 uiSize;

    public IObservable<int> OnPress => Observable.Merge(panels.Select(panel => panel.OnPress));
    public IObservable<int> OnRelease => Observable.Merge(panels.Select(panel => panel.OnRelease));

    private int currentSelected;

    public EquipItemsHandler(RectTransform rtEquipItems, Vector2 parentOrigin)
    {
        equips = new IReactiveProperty<ItemIcon>[] { equipL, equipBody, equipR };

        MAX_ITEMS = 3;

        inventoryRT = rtEquipItems;
        inventoryTf = rtEquipItems.transform;
        uiSize = rtEquipItems.sizeDelta;

        unit = new Vector2(uiSize.x / MAX_ITEMS, uiSize.y);

        // Anchor of ItemIcon is set to left top by default on prefab
        offsetCenter = new Vector2(unit.x, -unit.y) * 0.5f;

        UpdateOrigin();
        offsetOrigin = inventoryOrigin - parentOrigin;

        itemEmptyCheck = Enumerable.Repeat<IDisposable>(null, MAX_ITEMS).ToArray();

        currentSelected = MAX_ITEMS;

        resourceLoader = ResourceLoader.Instance;

        currentEquipments.Value = knuckleKnuckle = new KnuckleKnuckle(this);
        swordKnuckle = new SwordKnuckle(this);
        knuckleShield = new KnuckleShield(this);
        swordShield = new SwordShield(this);

    }

    public void UpdateOrigin()
    {
        inventoryOrigin = new Vector2(inventoryRT.position.x - uiSize.x * 0.5f, inventoryRT.position.y + uiSize.y * 0.5f);
    }

    public void ExpandNum(int index)
    {
        if (currentSelected < MAX_ITEMS) panels[currentSelected].ShrinkNum();
        if (index < MAX_ITEMS) panels[index].ExpandNum(inventoryTf);
        currentSelected = index;
    }

    public void DeleteNum(int index)
    {
        if (index < MAX_ITEMS) panels[index].SetItemNum(0);
    }

    public EquipItemsHandler SetPanels(ItemPanel prefabItemPanel)
    {
        panels = Enumerable
            .Range(0, MAX_ITEMS)
            .Select(
                index => Util.Instantiate(prefabItemPanel, inventoryTf, false)
                    .SetPos(LocalUIPos(index))
                    .SetIndex(index)
            )
            .ToArray();

        return this;
    }

    public void SetEnablePanels(bool isEnable) => panels.ForEach(panel => panel.SetEnabled(isEnable));

    public Vector2 ConvertToVec(Vector2 screenPos) => screenPos - inventoryOrigin;
    public bool IsOnUI(Vector2 uiPos) => uiPos.x >= 0f && uiPos.x <= uiSize.x && uiPos.y <= 0f && uiPos.y >= -uiSize.y;

    protected Vector2 LocalUIPos(int index) => offsetCenter + new Vector2(unit.x * index, 0f);
    public Vector2 UIPos(int index) => offsetOrigin + LocalUIPos(index);

    public void SetItem(ItemIcon itemIcon) => SetItem(itemIcon.index, itemIcon);

    public void SetItem(int index, ItemIcon itemIcon = null, bool tweenMove = false)
    {
        this.tweenMove = tweenMove;
        currentEquipments.Value = currentEquipments.Value.Equip(index, itemIcon);
    }

    public bool UpdateItemNum(ItemIcon itemIcon)
    {
        if (GetEquip(itemIcon.index) == null) return false;
        panels[itemIcon.index].SetItemNum(itemIcon.itemInfo.numOfItem);
        return true;
    }

    public ItemIcon GetItem(int index) => index < MAX_ITEMS ? GetEquip(index) : null;

    public bool RemoveItem(int index) => RemoveItem(GetItem(index));

    public bool RemoveItem(ItemIcon itemIcon)
    {
        if (itemIcon == null) return false;

        itemEmptyCheck[itemIcon.index]?.Dispose();
        itemEmptyCheck[itemIcon.index] = null;

        itemIcon.Inactivate();
        SetEquip(itemIcon.index, null);
        panels[itemIcon.index].SetItemNum(0);
        return true;
    }

    public void ForEach(Action<ItemIcon> action)
    {
        for (int i = 0; i < MAX_ITEMS; i++)
        {
            action(GetItem(i));
        }
    }

    public T[] Select<T>(Func<ItemIcon, T> func)
    {
        T[] ret = new T[MAX_ITEMS];

        for (int i = 0; i < MAX_ITEMS; i++)
        {
            ret[i] = func(GetItem(i));
        }

        return ret;
    }

    public ItemIcon[] Where(Func<ItemIcon, bool> func)
    {
        var icons = new List<ItemIcon>();

        for (int i = 0; i < MAX_ITEMS; i++)
        {
            var icon = GetItem(i);
            if (func(icon)) icons.Add(icon);
        }

        return icons.ToArray();
    }

    public DataStoreAgent.ItemInfo[] ExportAllItemInfo()
        => equips.Select(equip => equip.Value == null ? null : new DataStoreAgent.ItemInfo(equip.Value.itemInfo.type, equip.Value.itemInfo.numOfItem)).ToArray();

    protected abstract class EquipmentStyle : IEquipmentStyle
    {
        protected EquipItemsHandler equipments;
        protected AttackButtonsHandler prefabAttackButtonsHandler;
        protected InputRegion prefabInputRegion;
        protected FightStyleHandler prefabAttackStyleHandler;
        public RuntimeAnimatorController animatorController { get; protected set; }

        public AttackButtonsHandler LoadAttackButtonsHandler(Transform attackInputUI)
        {
            var instance = Util.Instantiate(prefabAttackButtonsHandler, attackInputUI);
            instance.transform.SetAsFirstSibling();
            return instance;
        }

        public InputRegion LoadInputRegion(Transform fightCircle)
        {
            var instance = UnityEngine.Object.Instantiate(prefabInputRegion, fightCircle);
            instance.transform.SetAsLastSibling();
            return instance;
        }

        public FightStyleHandler LoadFightStyle(Transform player)
        {
            var instance = Util.Instantiate(prefabAttackStyleHandler, player);
            instance.transform.SetAsLastSibling();
            return instance;
        }

        public EquipmentStyle(EquipItemsHandler equipments, string name)
        {
            this.equipments = equipments;
            LoadPrefabs(name);
        }

        protected void LoadPrefabs(string name)
        {
            prefabAttackButtonsHandler = Resources.Load<AttackButtonsHandler>($"Prefabs/UI/Fight/{name}ButtonsHandler");
            prefabInputRegion = Resources.Load<InputRegion>($"Prefabs/UI/Fight/{name}InputRegion");
            prefabAttackStyleHandler = Resources.Load<FightStyleHandler>($"Prefabs/Character/Player/{name}StyleHandler");
            animatorController = Resources.Load<RuntimeAnimatorController>($"AnimatorController/UnityChan_{name}");
        }

        public IEquipmentStyle Equip(int index, ItemIcon itemIcon)
        {
            if (index == 2) return Equip(itemIcon, EquipKnuckleR, EquipSwordR, EquipShieldR);
            if (index == 0) return Equip(itemIcon, EquipKnuckleL, EquipSwordL, EquipShieldL);

            throw new ArgumentException("Invalid index: " + index);
        }

        protected IEquipmentStyle Equip(ItemIcon itemIcon, params Func<ItemIcon, IEquipmentStyle>[] equipFuncs)
        {
            var source = equipments.GetEquipmentSource(itemIcon);
            var category = source != null ? source.category : EquipmentCategory.Knuckle;

            try { return equipFuncs[(int)category](itemIcon); }
            catch (IndexOutOfRangeException ex) { throw new ArgumentException(ex.Message + "\nInvalid equipment category: " + category); }
        }

        protected abstract IEquipmentStyle EquipKnuckleR(ItemIcon itemIcon);
        protected abstract IEquipmentStyle EquipSwordR(ItemIcon itemIcon);
        protected abstract IEquipmentStyle EquipShieldR(ItemIcon itemIcon);

        protected abstract IEquipmentStyle EquipKnuckleL(ItemIcon itemIcon);
        protected abstract IEquipmentStyle EquipSwordL(ItemIcon itemIcon);
        protected abstract IEquipmentStyle EquipShieldL(ItemIcon itemIcon);

        protected IEquipmentStyle SetR(ItemIcon itemIcon)
        {
            equipments.SetEquipR(itemIcon);
            return this;
        }

        protected IEquipmentStyle SetL(ItemIcon itemIcon)
        {
            equipments.SetEquipL(itemIcon);
            return this;
        }

        protected ItemIcon GetR() => equipments.GetEquipR();
        protected ItemIcon GetL() => equipments.GetEquipL();
    }

    protected class KnuckleKnuckle : EquipmentStyle
    {
        public KnuckleKnuckle(EquipItemsHandler equipments) : base(equipments, "KnuckleKnuckle") { }

        protected override IEquipmentStyle EquipKnuckleR(ItemIcon itemIcon)
        {
            return SetR(itemIcon);
        }
        protected override IEquipmentStyle EquipSwordR(ItemIcon itemIcon)
        {
            SetR(itemIcon);
            return equipments.swordKnuckle;
        }
        protected override IEquipmentStyle EquipShieldR(ItemIcon itemIcon)
        {
            SetR(GetL() ?? GetR());
            SetL(itemIcon);
            return equipments.knuckleShield;
        }

        protected override IEquipmentStyle EquipKnuckleL(ItemIcon itemIcon)
        {
            return SetL(itemIcon);
        }
        protected override IEquipmentStyle EquipSwordL(ItemIcon itemIcon)
        {
            SetL(GetR() ?? GetL());
            SetR(itemIcon);
            return equipments.swordKnuckle;
        }
        protected override IEquipmentStyle EquipShieldL(ItemIcon itemIcon)
        {
            SetL(itemIcon);
            return equipments.knuckleShield;
        }
    }

    protected class SwordKnuckle : EquipmentStyle
    {
        public SwordKnuckle(EquipItemsHandler equipments) : base(equipments, "SwordKnuckle") { }
        protected override IEquipmentStyle EquipKnuckleR(ItemIcon itemIcon)
        {
            SetR(itemIcon);
            return equipments.knuckleKnuckle;
        }
        protected override IEquipmentStyle EquipSwordR(ItemIcon itemIcon)
        {
            return SetR(itemIcon);
        }
        protected override IEquipmentStyle EquipShieldR(ItemIcon itemIcon)
        {
            SetL(itemIcon);
            return equipments.swordShield;
        }

        protected override IEquipmentStyle EquipKnuckleL(ItemIcon itemIcon)
        {
            return SetL(itemIcon);
        }
        protected override IEquipmentStyle EquipSwordL(ItemIcon itemIcon)
        {
            return SetR(itemIcon);
        }
        protected override IEquipmentStyle EquipShieldL(ItemIcon itemIcon)
        {
            SetL(itemIcon);
            return equipments.swordShield;
        }
    }

    protected class KnuckleShield : EquipmentStyle
    {
        public KnuckleShield(EquipItemsHandler equipments) : base(equipments, "KnuckleShield") { }
        protected override IEquipmentStyle EquipKnuckleR(ItemIcon itemIcon)
        {
            return SetR(itemIcon);
        }
        protected override IEquipmentStyle EquipSwordR(ItemIcon itemIcon)
        {
            SetR(itemIcon);
            return equipments.swordShield;
        }
        protected override IEquipmentStyle EquipShieldR(ItemIcon itemIcon)
        {
            return SetL(itemIcon);
        }

        protected override IEquipmentStyle EquipKnuckleL(ItemIcon itemIcon)
        {
            SetL(itemIcon);
            return equipments.knuckleKnuckle;
        }
        protected override IEquipmentStyle EquipSwordL(ItemIcon itemIcon)
        {
            SetR(itemIcon);
            return equipments.swordShield;
        }
        protected override IEquipmentStyle EquipShieldL(ItemIcon itemIcon)
        {
            return SetL(itemIcon);
        }
    }

    protected class SwordShield : EquipmentStyle
    {
        public SwordShield(EquipItemsHandler equipments) : base(equipments, "SwordShield") { }
        protected override IEquipmentStyle EquipKnuckleR(ItemIcon itemIcon)
        {
            SetR(itemIcon);
            return equipments.knuckleShield;
        }
        protected override IEquipmentStyle EquipSwordR(ItemIcon itemIcon)
        {
            return SetR(itemIcon);
        }
        protected override IEquipmentStyle EquipShieldR(ItemIcon itemIcon)
        {
            return SetL(itemIcon);
        }

        protected override IEquipmentStyle EquipKnuckleL(ItemIcon itemIcon)
        {
            SetL(itemIcon);
            return equipments.swordKnuckle;
        }
        protected override IEquipmentStyle EquipSwordL(ItemIcon itemIcon)
        {
            return SetR(itemIcon);
        }
        protected override IEquipmentStyle EquipShieldL(ItemIcon itemIcon)
        {
            return SetL(itemIcon);
        }
    }
}
