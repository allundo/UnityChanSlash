using UnityEngine;
using UniRx;
using System.Linq;
using System;

public interface IEquipmentStyle
{
    IEquipmentStyle Equip(int index, ItemIcon itemIcon);
    AttackButtonsHandler LoadAttackButtonsHandler(Transform attackInputUI);
    InputRegion LoadInputRegion(Transform fightCircle);
    FightStyleHandler LoadFightStyle(Transform player);
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
        currentEquipments.Value = currentEquipments.Value.Equip(index, itemIcon);
    }
    protected override void StoreItem(int index, ItemIcon itemIcon)
    {
        itemIcon?.SetInventoryType(true);
        equips[index].Value = itemIcon;
    }

    protected void SetEquip(int index, ItemIcon itemIcon) => SetItemWithEmptyCheck(index, itemIcon, tweenMove);
    protected void SetEquipR(ItemIcon itemIcon) => SetEquip(2, itemIcon);
    protected void SetEquipL(ItemIcon itemIcon) => SetEquip(0, itemIcon);

    protected ItemIcon GetEquipR() => equipR.Value;
    protected ItemIcon GetEquipL() => equipL.Value;

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

        public IEquipmentStyle Equip(int index, ItemIcon itemIcon)
        {
            if (index == 2) return Equip(itemIcon, EquipKnuckleR, EquipSwordR, EquipShieldR);
            if (index == 0) return Equip(itemIcon, EquipKnuckleL, EquipSwordL, EquipShieldL);

            throw new ArgumentException("Invalid index: " + index);
        }

        protected IEquipmentStyle Equip(ItemIcon itemIcon, params Func<ItemIcon, IEquipmentStyle>[] equipFuncs)
        {
            var category = ResourceLoader.Instance.GetEquipmentOrDefault(itemIcon).category;
            return equipFuncs[(int)category](itemIcon);
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
