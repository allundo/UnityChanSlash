using UnityEngine;
using System;
using UniRx;

public interface IEquipments
{
    IEquipments EquipR(EquipmentSource source);
    IEquipments EquipL(EquipmentSource source);
    AttackButtonsHandler LoadAttackButtonsHandler(Transform attackInputUI);
    InputRegion LoadInputRegion(Transform fightCircle);
}

public class HandEquipments
{
    private IReactiveProperty<IEquipments> currentEquipments = new ReactiveProperty<IEquipments>();
    public IObservable<IEquipments> CurrentEquipments => currentEquipments;

    protected KnuckleKnuckle knuckleKnuckle = null;
    protected SwordKnuckle swordKnuckle = null;
    protected KnuckleShield knuckleShield = null;
    protected SwordShield swordShield = null;

    private IReactiveProperty<EquipmentSource> sourceRRP;
    private IReactiveProperty<EquipmentSource> sourceLRP;

    public IObservable<EquipmentSource> SourceR => sourceRRP;
    public IObservable<EquipmentSource> SourceL => sourceLRP;

    public EquipmentSource sourceR
    {
        get { return sourceRRP.Value; }
        protected set { sourceRRP.Value = value; }
    }

    public EquipmentSource sourceL
    {
        get { return sourceLRP.Value; }
        protected set { sourceLRP.Value = value; }
    }

    public HandEquipments()
    {
        currentEquipments.Value = knuckleKnuckle = new KnuckleKnuckle(this);
        swordKnuckle = new SwordKnuckle(this);
        knuckleShield = new KnuckleShield(this);
        swordShield = new SwordShield(this);

        var sourceBareHand = new EquipmentSource()
        {
            attackMultiplier = 1f,
            shieldPlusL = 0f,
            shieldPlusR = 0f,
            category = EquipmentCategory.Knuckle
        };

        sourceRRP = new ReactiveProperty<EquipmentSource>(sourceBareHand);
        sourceLRP = new ReactiveProperty<EquipmentSource>(sourceBareHand);
    }

    private bool Equip(ItemType type, Func<EquipmentSource, IEquipments> equipFunc)
    {
        var source = ResourceLoader.Instance.GetEquipmentSource(type);
        if (source == null) return false;

        currentEquipments.Value = equipFunc(source);
        return true;
    }

    public bool EquipR(ItemType type) => Equip(type, currentEquipments.Value.EquipR);
    public bool EquipL(ItemType type) => Equip(type, currentEquipments.Value.EquipL);

    protected class KnuckleKnuckle : IEquipments
    {
        protected HandEquipments equipments;
        protected AttackButtonsHandler prefabAttackButtonsHandler;
        protected InputRegion prefabInputRegion;
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

        public KnuckleKnuckle(HandEquipments equipments, string name = "KnuckleKnuckle")
        {
            this.equipments = equipments;
            LoadPrefabs(name);
        }

        protected void LoadPrefabs(string name)
        {
            prefabAttackButtonsHandler = Resources.Load<AttackButtonsHandler>($"Prefabs/UI/Fight/{name}ButtonsHandler");
            prefabInputRegion = Resources.Load<InputRegion>($"Prefabs/UI/Fight/{name}InputRegion");
        }

        public virtual IEquipments EquipR(EquipmentSource source)
        {
            switch (source.category)
            {
                case EquipmentCategory.Knuckle:
                    return SetR(source);

                case EquipmentCategory.Sword:
                    equipments.sourceR = source;
                    return equipments.swordKnuckle;

                case EquipmentCategory.Shield:
                    equipments.sourceR = equipments.sourceR ?? equipments.sourceL;
                    equipments.sourceL = source;
                    return equipments.knuckleShield;
            }

            throw new ArgumentException("Equipment category isn't supported: " + source.category);
        }

        public virtual IEquipments EquipL(EquipmentSource source)
        {
            switch (source.category)
            {
                case EquipmentCategory.Knuckle:
                    return SetL(source);

                case EquipmentCategory.Sword:
                    equipments.sourceL = equipments.sourceL ?? equipments.sourceR;
                    equipments.sourceR = source;
                    return equipments.swordKnuckle;

                case EquipmentCategory.Shield:
                    equipments.sourceR = equipments.sourceR ?? equipments.sourceL;
                    equipments.sourceL = source;
                    return equipments.knuckleShield;
            }

            throw new ArgumentException("Equipment category isn't supported: " + source.category);
        }

        protected IEquipments SetR(EquipmentSource source)
        {
            equipments.sourceR = source;
            return this;
        }

        protected IEquipments SetL(EquipmentSource source)
        {
            equipments.sourceL = source;
            return this;
        }
    }

    protected class SwordKnuckle : KnuckleKnuckle
    {
        public SwordKnuckle(HandEquipments equipments) : base(equipments, "SwordKnuckle") { }

        public override IEquipments EquipR(EquipmentSource source)
        {
            switch (source.category)
            {
                case EquipmentCategory.Knuckle:
                    equipments.sourceR = source;
                    return equipments.knuckleKnuckle;

                case EquipmentCategory.Sword:
                    return SetR(source);

                case EquipmentCategory.Shield:
                    equipments.sourceL = source;
                    return equipments.swordShield;
            }

            throw new ArgumentException("Equipment category isn't supported: " + source.category);
        }

        public override IEquipments EquipL(EquipmentSource source)
        {
            switch (source.category)
            {
                case EquipmentCategory.Knuckle:
                    return SetL(source);

                case EquipmentCategory.Sword:
                    return SetR(source);

                case EquipmentCategory.Shield:
                    equipments.sourceL = source;
                    return equipments.swordShield;
            }

            throw new ArgumentException("Equipment category isn't supported: " + source.category);
        }
    }

    protected class KnuckleShield : KnuckleKnuckle
    {
        public KnuckleShield(HandEquipments equipments) : base(equipments, "KnuckleShield") { }

        public override IEquipments EquipR(EquipmentSource source)
        {
            switch (source.category)
            {
                case EquipmentCategory.Knuckle:
                    return SetR(source);

                case EquipmentCategory.Sword:
                    equipments.sourceR = source;
                    return equipments.swordShield;

                case EquipmentCategory.Shield:
                    return SetL(source);
            }

            throw new ArgumentException("Equipment category isn't supported: " + source.category);
        }

        public override IEquipments EquipL(EquipmentSource source)
        {
            switch (source.category)
            {
                case EquipmentCategory.Knuckle:
                    equipments.sourceL = source;
                    return equipments.knuckleKnuckle;

                case EquipmentCategory.Sword:
                    equipments.sourceR = source;
                    return equipments.swordShield;

                case EquipmentCategory.Shield:
                    return SetL(source);
            }

            throw new ArgumentException("Equipment category isn't supported: " + source.category);
        }
    }
    protected class SwordShield : KnuckleKnuckle
    {
        public SwordShield(HandEquipments equipments) : base(equipments, "SwordShield") { }

        public override IEquipments EquipR(EquipmentSource source)
        {
            switch (source.category)
            {
                case EquipmentCategory.Knuckle:
                    equipments.sourceR = source;
                    return equipments.knuckleShield;

                case EquipmentCategory.Sword:
                    return SetR(source);

                case EquipmentCategory.Shield:
                    return SetL(source);
            }

            throw new ArgumentException("Equipment category isn't supported: " + source.category);
        }

        public override IEquipments EquipL(EquipmentSource source)
        {
            switch (source.category)
            {
                case EquipmentCategory.Knuckle:
                    equipments.sourceL = source;
                    return equipments.swordKnuckle;

                case EquipmentCategory.Sword:
                    return SetR(source);

                case EquipmentCategory.Shield:
                    return SetL(source);
            }

            throw new ArgumentException("Equipment category isn't supported: " + source.category);
        }
    }
}
