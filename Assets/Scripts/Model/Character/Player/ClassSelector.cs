using UnityEngine;
using System.Linq;

public class ClassSelector
{
    public interface IClassSelector
    {
        IClassSelector SelectType(float attack, float shield, float damage, float magic, float magicDamage);
        LevelGainType type { get; }
    }
    public LevelGainType type => currentSelector.type;
    public LevelGainData levelGainData { get; private set; }

    private IClassSelector currentSelector;
    protected IClassSelector balance;
    protected IClassSelector attacker;
    protected IClassSelector shielder;
    protected IClassSelector guardian;
    protected IClassSelector magician;
    protected IClassSelector berserker;
    protected IClassSelector[] selectors;

    public ClassSelector()
    {
        levelGainData = Resources.Load<LevelGainData>("DataAssets/Character/LevelGainData");

        currentSelector = balance = new Balance(this);
        attacker = new Attacker(this);
        shielder = new Shielder(this);
        guardian = new Guardian(this);
        magician = new Magician(this);
        berserker = new Berserker(this);
        selectors = new IClassSelector[] { balance, attacker, shielder, guardian, magician, berserker };
    }

    public LevelGain SelectType(PlayerCounter counter)
    {
        var selector =
            currentSelector.SelectType
            (
                counter.Attack * 1f,
                counter.Shield * 1.5f,
                counter.Damage * 1f,
                counter.Magic * 2f,
                counter.MagicDamage * 2f
            );

        var levelGain = levelGainData.Param((int)selector.type);

        if (selector != currentSelector)
        {
            currentSelector = selector;
            ActiveMessageController.Instance.ClassChange(levelGain.name);
        }

        return levelGain;
    }

    public LevelGain SetSelector(LevelGainType type)
    {
        currentSelector = selectors[(int)type];
        return levelGainData.Param((int)type);
    }

    protected class Balance : IClassSelector
    {
        protected ClassSelector parent;
        public LevelGainType type { get; }

        public Balance(ClassSelector parent, LevelGainType type = LevelGainType.Balance)
        {
            this.parent = parent;
            this.type = type;
        }

        private static float THRESHOLD = 1.5f;

        public virtual IClassSelector SelectType(float attack, float shield, float damage, float magic, float magicDamage)
        {
            float sum = new float[] { attack, shield, magic, damage, magicDamage }.Sum();

            var ratio = new float[]
            {
                attack,
                shield,
                shield * 0.25f + damage * 0.5f + magicDamage * 0.25f,
                magic * 0.75f + magicDamage * 0.25f,
                attack * 0.5f + damage * 0.25f + magicDamage * 0.25f,
            }
            .Select(element => element / sum)
            .ToList();

            float minRatio = ratio.Min();
            float maxRatio = ratio.Max();
            int maxIndex = ratio.IndexOf(maxRatio);
            ratio.RemoveAt(maxIndex);

            float secondRatio = ratio.Max();

            if (secondRatio == 0f || maxRatio / secondRatio > THRESHOLD) return parent.selectors[maxIndex + 1];
            if (minRatio > 0f && maxRatio / minRatio < THRESHOLD) return parent.balance;

            return this;
        }
    }

    protected class Attacker : Balance
    {
        public Attacker(ClassSelector parent) : base(parent, LevelGainType.Attacker) { }
        public override IClassSelector SelectType(float attack, float shield, float damage, float magic, float magicDamage)
            => base.SelectType(10f + attack * 1.5f, shield, damage, magic, magicDamage);
    }

    protected class Shielder : Balance
    {
        public Shielder(ClassSelector parent) : base(parent, LevelGainType.Shielder) { }
        public override IClassSelector SelectType(float attack, float shield, float damage, float magic, float magicDamage)
            => base.SelectType(attack, 10f + shield * 1.5f, damage, magic, magicDamage);
    }

    protected class Guardian : Balance
    {
        public Guardian(ClassSelector parent) : base(parent, LevelGainType.Guardian) { }
        public override IClassSelector SelectType(float attack, float shield, float damage, float magic, float magicDamage)
            => base.SelectType(attack, 2.5f + shield * 1.5f, 5f + damage * 1.5f, magic, 2.5f + magicDamage * 1.5f);
    }

    protected class Magician : Balance
    {
        public Magician(ClassSelector parent) : base(parent, LevelGainType.Magician) { }
        public override IClassSelector SelectType(float attack, float shield, float damage, float magic, float magicDamage)
            => base.SelectType(attack, shield, damage, 7.5f + magic * 1.5f, 2.5f + magicDamage * 1.5f);
    }

    protected class Berserker : Balance
    {
        public Berserker(ClassSelector parent) : base(parent, LevelGainType.Berserker) { }
        public override IClassSelector SelectType(float attack, float shield, float damage, float magic, float magicDamage)
            => base.SelectType(5f + attack * 1.5f, shield, 2.5f + damage * 1.5f, magic, 2.5f + magicDamage * 1.5f);
    }
}