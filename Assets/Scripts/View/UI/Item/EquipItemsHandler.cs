using UnityEngine;
using UniRx;
using System.Linq;
using System;

public interface IEquipmentStyle
{
    IEquipmentStyle Equip(EquipmentCategory category, ItemIcon itemIcon);
    IEquipmentStyle Equip(int index, ItemIcon itemIcon);
    AttackButtonsHandler LoadAttackButtonsHandler(Transform attackInputUI);
    InputRegion LoadInputRegion(Transform fightCircle);
    FightStyleHandler LoadFightStyle(Transform player);
    IEquipmentStyle DetectEquipmentStyle();
    RuntimeAnimatorController animatorController { get; }
}

public class EquipItemsHandler : ItemIndexHandler
{
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

    public EquipItemsHandler(ItemInventory inventory, RectTransform equipItemsRT, ItemPanel prefabEquipPanel)
        : base(inventory, equipItemsRT, prefabEquipPanel, 3, 1)
    {
        equips = new IReactiveProperty<ItemIcon>[] { equipL, equipBody, equipR };

        currentEquipments.Value = knuckleKnuckle = new KnuckleKnuckle(this);
        swordKnuckle = new SwordKnuckle(this);
        knuckleShield = new KnuckleShield(this);
        swordShield = new SwordShield(this);
    }

    public override void ResetOrientation(DeviceOrientation orientation) => UpdateOrigin();

    protected override Vector2 GetOffsetOrigin() => uiOrigin - inventory.uiOrigin;
    protected override ItemIcon[] Items => equips.Select(equip => equip.Value).ToArray();
    public bool isEnable { get; protected set; } = true;
    public override void SetEnablePanels(bool isEnable)
    {
        this.isEnable = isEnable;
        panels.ForEach(panel => panel.SetEnabled(isEnable));
    }

    private bool tweenMove = false;

    public override void SetItem(int index, ItemIcon itemIcon, bool tweenMove = false)
    {
        this.tweenMove = tweenMove;
        SetEquip(index, itemIcon);
        currentEquipments.Value = currentEquipments.Value.DetectEquipmentStyle();
    }
    public override void SwitchItem(int index, ItemIcon itemIcon) => Equip(index, itemIcon, true);
    private void Equip(int index, ItemIcon itemIcon, bool tweenMove = false)
    {
        this.tweenMove = tweenMove;
        currentEquipments.Value = currentEquipments.Value.Equip(index, itemIcon);
    }

    public void Equip(EquipmentCategory category, ItemIcon itemIcon)
    {
        tweenMove = true;
        currentEquipments.Value = currentEquipments.Value.Equip(category, itemIcon);
    }

    protected override void StoreItem(int index, ItemIcon itemIcon)
    {
        itemIcon?.SetInventoryType(true);
        equips[index].Value = itemIcon;
    }

    protected void SetEquip(int index, ItemIcon itemIcon) => SetItemWithEmptyCheck(index, itemIcon, tweenMove);

    protected override Vector2 LocalUIPos(int index) => LocalUIPos(index, 0);
    protected override Vector2 LocalUIPos(int x, int y) => panelOffsetCenter + new Vector2(panelUnit.x * x, 0);

    protected abstract class EquipmentStyle : IEquipmentStyle
    {
        protected EquipItemsHandler equipments;
        protected AttackButtonsHandler prefabAttackButtonsHandler;
        protected InputRegion prefabInputRegion;
        protected FightStyleHandler prefabAttackStyleHandler;
        public RuntimeAnimatorController animatorController { get; protected set; }

        private T Instantiate<T>(T prefab, Transform parent) where T : UnityEngine.Object
        {
            bool isActive = parent.gameObject.activeSelf;

            // Make sure to call Awake()
            parent.gameObject.SetActive(true);
            var instance = Util.Instantiate(prefab, parent);
            parent.gameObject.SetActive(isActive);

            return instance;
        }

        public AttackButtonsHandler LoadAttackButtonsHandler(Transform attackInputUI)
        {
            var instance = Instantiate(prefabAttackButtonsHandler, attackInputUI);
            instance.transform.SetAsFirstSibling();
            return instance;
        }

        public InputRegion LoadInputRegion(Transform fightCircle)
        {
            var instance = Instantiate(prefabInputRegion, fightCircle);
            instance.transform.SetAsLastSibling();
            return instance;
        }

        public FightStyleHandler LoadFightStyle(Transform player)
        {
            var instance = Instantiate(prefabAttackStyleHandler, player);
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

        public IEquipmentStyle DetectEquipmentStyle()
        {
            var resourceLoader = ResourceLoader.Instance;

            var categoryR = resourceLoader.GetEquipmentOrDefault(GetR()).category;
            var categoryL = resourceLoader.GetEquipmentOrDefault(GetL()).category;

            if (categoryR == EquipmentCategory.Sword)
            {
                if (categoryL == EquipmentCategory.Knuckle) return equipments.swordKnuckle;
                if (categoryL == EquipmentCategory.Shield) return equipments.swordShield;
            }

            if (categoryR == EquipmentCategory.Knuckle)
            {
                if (categoryL == EquipmentCategory.Knuckle) return equipments.knuckleKnuckle;
                if (categoryL == EquipmentCategory.Shield) return equipments.knuckleShield;
            }

            throw new Exception($"invalid equipment style: {categoryR} {categoryL}");
        }

        public IEquipmentStyle Equip(EquipmentCategory category, ItemIcon itemIcon)
        {
            switch (category)
            {
                case EquipmentCategory.Sword:
                    return EquipSwordR(itemIcon);

                case EquipmentCategory.Knuckle:
                    return GetR() == null || GetL() != null ? EquipKnuckleR(itemIcon) : EquipKnuckleOpenHand(itemIcon);

                case EquipmentCategory.Shield:
                    return EquipShieldL(itemIcon);

                case EquipmentCategory.Amulet:
                    return SwitchBody(itemIcon);
            }

            throw new ArgumentException("Invalid category: " + category);
        }

        public IEquipmentStyle Equip(int index, ItemIcon itemIcon)
        {
            if (itemIcon.isEquip) return Exchange(index, itemIcon);

            switch (index)
            {
                case 2: return Equip(itemIcon, EquipKnuckleR, EquipSwordR, EquipShieldR);
                case 0: return Equip(itemIcon, EquipKnuckleL, EquipSwordL, EquipShieldL);
                case 1: return SwitchBody(itemIcon);
            }

            throw new ArgumentException("Invalid index: " + index);
        }

        protected IEquipmentStyle Equip(ItemIcon itemIcon, params Func<ItemIcon, IEquipmentStyle>[] equipFuncs)
        {
            var category = ResourceLoader.Instance.GetEquipmentOrDefault(itemIcon).category;
            return equipFuncs[(int)category](itemIcon);
        }

        protected virtual IEquipmentStyle Exchange(int index, ItemIcon itemIcon) => Switch(index, itemIcon);

        protected abstract IEquipmentStyle EquipKnuckleR(ItemIcon itemIcon);
        protected virtual IEquipmentStyle EquipKnuckleOpenHand(ItemIcon itemIcon) => EquipKnuckleR(itemIcon);
        protected abstract IEquipmentStyle EquipSwordR(ItemIcon itemIcon);
        protected abstract IEquipmentStyle EquipShieldR(ItemIcon itemIcon);

        protected abstract IEquipmentStyle EquipKnuckleL(ItemIcon itemIcon);
        protected abstract IEquipmentStyle EquipSwordL(ItemIcon itemIcon);
        protected abstract IEquipmentStyle EquipShieldL(ItemIcon itemIcon);

        private IEquipmentStyle Switch(int index, ItemIcon itemIcon)
        {
            equipments.setBackSubject.OnNext((itemIcon, Get(index)));
            return Set(index, itemIcon);
        }

        protected IEquipmentStyle SwitchR(ItemIcon itemIcon) => Switch(2, itemIcon);
        protected IEquipmentStyle SwitchL(ItemIcon itemIcon) => Switch(0, itemIcon);
        protected IEquipmentStyle SwitchBody(ItemIcon itemIcon) => Switch(1, itemIcon);

        protected IEquipmentStyle Set(int index, ItemIcon itemIcon)
        {
            equipments.SetEquip(index, itemIcon);
            return this;
        }

        protected IEquipmentStyle SetR(ItemIcon itemIcon) => Set(2, itemIcon);
        protected IEquipmentStyle SetL(ItemIcon itemIcon) => Set(0, itemIcon);
        protected IEquipmentStyle SetBody(ItemIcon itemIcon) => Set(1, itemIcon);

        private ItemIcon Get(int index) => equipments.equips[index].Value;
        protected ItemIcon GetR() => Get(2);
        protected ItemIcon GetL() => Get(0);
        protected ItemIcon GetBody() => Get(1);
    }

    protected class KnuckleKnuckle : EquipmentStyle
    {
        public KnuckleKnuckle(EquipItemsHandler equipments) : base(equipments, "KnuckleKnuckle") { }

        protected override IEquipmentStyle EquipKnuckleOpenHand(ItemIcon itemIcon) => SwitchL(itemIcon);

        protected override IEquipmentStyle EquipKnuckleR(ItemIcon itemIcon)
        {
            return SwitchR(itemIcon);
        }
        protected override IEquipmentStyle EquipSwordR(ItemIcon itemIcon)
        {
            SwitchR(itemIcon);
            return equipments.swordKnuckle;
        }
        protected override IEquipmentStyle EquipShieldR(ItemIcon itemIcon)
        {
            SetR(GetL() ?? GetR());
            SwitchL(itemIcon);
            return equipments.knuckleShield;
        }

        protected override IEquipmentStyle EquipKnuckleL(ItemIcon itemIcon)
        {
            return SwitchL(itemIcon);
        }
        protected override IEquipmentStyle EquipSwordL(ItemIcon itemIcon)
        {
            SetL(GetR() ?? GetL());
            if (GetR() == GetL()) SetR(null);
            SwitchR(itemIcon);
            return equipments.swordKnuckle;
        }
        protected override IEquipmentStyle EquipShieldL(ItemIcon itemIcon)
        {
            SwitchL(itemIcon);
            return equipments.knuckleShield;
        }
    }

    protected class SwordKnuckle : EquipmentStyle
    {
        public SwordKnuckle(EquipItemsHandler equipments) : base(equipments, "SwordKnuckle") { }

        protected override IEquipmentStyle Exchange(int index, ItemIcon itemIcon) => Set(itemIcon.index, itemIcon);
        protected override IEquipmentStyle EquipKnuckleOpenHand(ItemIcon itemIcon) => SwitchL(itemIcon);

        protected override IEquipmentStyle EquipKnuckleR(ItemIcon itemIcon)
        {
            SwitchR(itemIcon);
            return equipments.knuckleKnuckle;
        }
        protected override IEquipmentStyle EquipSwordR(ItemIcon itemIcon)
        {
            return SwitchR(itemIcon);
        }
        protected override IEquipmentStyle EquipShieldR(ItemIcon itemIcon)
        {
            SwitchL(itemIcon);
            return equipments.swordShield;
        }

        protected override IEquipmentStyle EquipKnuckleL(ItemIcon itemIcon)
        {
            return SwitchL(itemIcon);
        }
        protected override IEquipmentStyle EquipSwordL(ItemIcon itemIcon)
        {
            return SwitchR(itemIcon);
        }
        protected override IEquipmentStyle EquipShieldL(ItemIcon itemIcon)
        {
            SwitchL(itemIcon);
            return equipments.swordShield;
        }
    }

    protected class KnuckleShield : EquipmentStyle
    {
        public KnuckleShield(EquipItemsHandler equipments) : base(equipments, "KnuckleShield") { }

        protected override IEquipmentStyle Exchange(int index, ItemIcon itemIcon) => Set(itemIcon.index, itemIcon);
        protected override IEquipmentStyle EquipKnuckleR(ItemIcon itemIcon)
        {
            return SwitchR(itemIcon);
        }
        protected override IEquipmentStyle EquipSwordR(ItemIcon itemIcon)
        {
            SwitchR(itemIcon);
            return equipments.swordShield;
        }
        protected override IEquipmentStyle EquipShieldR(ItemIcon itemIcon)
        {
            return SwitchL(itemIcon);
        }

        protected override IEquipmentStyle EquipKnuckleL(ItemIcon itemIcon)
        {
            SwitchL(itemIcon);
            return equipments.knuckleKnuckle;
        }
        protected override IEquipmentStyle EquipSwordL(ItemIcon itemIcon)
        {
            SwitchR(itemIcon);
            return equipments.swordShield;
        }
        protected override IEquipmentStyle EquipShieldL(ItemIcon itemIcon)
        {
            return SwitchL(itemIcon);
        }
    }

    protected class SwordShield : EquipmentStyle
    {
        public SwordShield(EquipItemsHandler equipments) : base(equipments, "SwordShield") { }

        protected override IEquipmentStyle Exchange(int index, ItemIcon itemIcon) => Set(itemIcon.index, itemIcon);
        protected override IEquipmentStyle EquipKnuckleR(ItemIcon itemIcon)
        {
            SwitchR(itemIcon);
            return equipments.knuckleShield;
        }
        protected override IEquipmentStyle EquipSwordR(ItemIcon itemIcon)
        {
            return SwitchR(itemIcon);
        }
        protected override IEquipmentStyle EquipShieldR(ItemIcon itemIcon)
        {
            return SwitchL(itemIcon);
        }

        protected override IEquipmentStyle EquipKnuckleL(ItemIcon itemIcon)
        {
            SwitchL(itemIcon);
            return equipments.swordKnuckle;
        }
        protected override IEquipmentStyle EquipSwordL(ItemIcon itemIcon)
        {
            return SwitchR(itemIcon);
        }
        protected override IEquipmentStyle EquipShieldL(ItemIcon itemIcon)
        {
            return SwitchL(itemIcon);
        }
    }
}
