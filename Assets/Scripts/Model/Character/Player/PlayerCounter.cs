using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerCounter
{
    [SerializeField] private int[] defeat = new int[Util.Count<EnemyType>() - 1];
    [SerializeField] private int[] attack = new int[Util.Count<EquipmentCategory>() - 1];
    [SerializeField] private int[] critical = new int[Util.Count<EquipmentCategory>() - 1];
    [SerializeField] private int shield = 0;
    [SerializeField] private int[] magic = new int[Util.Count<AttackAttr>() - 1];
    [SerializeField] private int damage = 0;
    [SerializeField] private int magicDamage = 0;

    public static List<string> Titles => TitleSelector.Titles;

    public int Defeat => defeat.Sum();
    public int AttackPoint => attack[0] + attack[1] + (critical[0] + critical[1]) * 2;
    public int ShieldPoint => shield + attack[2] + critical[2] * 2;
    public int Magic
    {
        get
        {
            int sum = 0;
            for (int i = 0; i < magic.Length - 1; i++)
            {
                sum += magic[i];
            }
            return sum;
        }
    }

    public int Damage => damage;
    public int MagicDamage => magicDamage;

    [SerializeField] private int[] defeatSum = new int[Util.Count<EnemyType>() - 1];
    [SerializeField] private int[] attackSum = new int[Util.Count<EquipmentCategory>() - 1];
    [SerializeField] private int[] criticalSum = new int[Util.Count<EquipmentCategory>() - 1];
    [SerializeField] private int shieldSum = 0;
    [SerializeField] private int[] magicSum = new int[Util.Count<AttackAttr>() - 1];
    [SerializeField] private int damageSum = 0;
    [SerializeField] private int magicDamageSum = 0;
    [SerializeField] private int potionSum = 0;
    [SerializeField] private int stepSum = 0;

    public int DefeatSum => defeatSum.Sum();
    public int DefeatType(EnemyType type) => defeatSum[(int)type - 1];
    public int AttackPointSum => attackSum[0] + attackSum[1] + (criticalSum[0] + criticalSum[1]) * 2;
    public int ShieldPointSum => shieldSum + attackSum[2] + criticalSum[2] * 2;
    public int MagicSum
    {
        get
        {
            int sum = 0;
            for (int i = 0; i < magicSum.Length - 1; i++)
            {
                sum += magicSum[i];
            }
            return sum;
        }
    }
    public int MagicType(AttackAttr attr) => magicSum[(int)attr - 1];
    public int DamageSum => damageSum;
    public int MagicDamageSum => magicDamageSum;
    public int CoinSum => magicSum[(int)AttackAttr.Coin - 1];
    public int PotionSum => potionSum;
    public int StepSum => stepSum;

    public void IncDefeat(EnemyType type = EnemyType.None)
    {
        if (type != EnemyType.None) ++defeat[(int)type - 1];
    }

    public void IncAttack(EquipmentCategory category, bool isCritical = false) => ++(isCritical ? critical : attack)[(int)category];
    public void IncShield() => ++shield;
    public void IncMagic(AttackAttr attr = AttackAttr.None)
    {
        if (attr != AttackAttr.None) ++magic[(int)attr - 1];
    }

    public void IncDamage() => ++damage;
    public void DecDamage() => --damage;
    public void IncMagicDamage() => ++magicDamage;
    public void IncPotion() => ++potionSum;
    public void IncStep() => ++stepSum;

    public void TotalCounts()
    {
        for (int i = 0; i < defeat.Length; i++)
        {
            defeatSum[i] += defeat[i];
            defeat[i] = 0;
        }

        for (int i = 0; i < attack.Length; i++)
        {
            attackSum[i] += attack[i];
            criticalSum[i] += critical[i];
            attack[i] = critical[i] = 0;
        }

        shieldSum += shield;

        for (int i = 0; i < magic.Length; i++)
        {
            magicSum[i] += magic[i];
            magic[i] = 0;
        }

        damageSum += damage;
        magicDamageSum += magicDamage;

        shield = damage = magicDamage = 0;
    }

    public ClearData TotalClearCounts(float mapComp, float treasureComp, float clearTimeSec)
    {
        var titleData = new TitleSelector(this, mapComp, treasureComp, clearTimeSec).titleData;
        return new ClearData()
        {
            title = titleData.title,
            bonusRate = titleData.bonusRate,
            defeatCount = DefeatSum,
        };
    }

    public struct ClearData
    {
        public string title;
        public float bonusRate;
        public int defeatCount;
    }

    protected class TitleSelector
    {
        public struct Title
        {
            public const string SlowPoke = "鈍足";
            public const string Fighter = "戦士";
            public const string Magician = "魔導士";
            public const string Shielder = "盾使い";
            public const string SwordFighter = "剣闘士";
            public const string Grappler = "格闘家";
            public const string FireMagician = "炎術士";
            public const string IceMagician = "氷術士";
            public const string DarkMagician = "闇術士";
            public const string Mapper = "測量士";
            public const string PhantomThief = "怪盗";
            public const string ShieldMaster = "盾師";
            public const string SwordMaster = "剣聖";
            public const string Kuroobi = "黒帯";
            public const string FastRunner = "俊足";
            public const string MinimumStep = "低歩数";
            public const string Zenigata = "銭形";
        }

        private PlayerCounter counter;
        private Dictionary<string, float> titlePoint = new Dictionary<string, float>();
        private static Dictionary<string, float> titleBonus = new Dictionary<string, float>()
        {
            { Title.SlowPoke,       0.0f    },
            { Title.Fighter,        0.015f  },
            { Title.Magician,       0.02f   },
            { Title.Shielder,       0.025f  },
            { Title.SwordFighter,   0.025f  },
            { Title.Grappler,       0.025f  },
            { Title.FireMagician,   0.05f   },
            { Title.IceMagician,    0.05f   },
            { Title.DarkMagician,   0.075f  },
            { Title.Mapper,         0.08f   },
            { Title.PhantomThief,   0.1f    },
            { Title.ShieldMaster,   0.15f   },
            { Title.SwordMaster,    0.15f   },
            { Title.Kuroobi,        0.15f   },
            { Title.FastRunner,     0.2f    },
            { Title.MinimumStep,    0.25f   },
            { Title.Zenigata,       0.5f    },
        };

        public static readonly List<string> Titles = titleBonus.Select(kv => kv.Key).ToList();

        public (string title, float bonusRate) titleData { get; private set; }

        public TitleSelector(PlayerCounter counter, float mapComp, float treasureComp, float clearTimeSec)
        {
            counter.TotalCounts();
            this.counter = counter;
            float attackSum = (float)(counter.attackSum.Sum() + counter.criticalSum.Sum());
            float magic = (float)counter.MagicSum;

            float fighter = attackSum > 0 ? attackSum / (magic + attackSum) : 0;
            float magician = magic > 0 ? 1f - fighter : 0;

            int attackLen = counter.attackSum.Length;
            float[] attackRate = new float[attackLen];
            float[] criticalRate = new float[attackLen];
            float[] categoryRate = new float[attackLen];
            for (int i = 0; i < attackLen; i++)
            {
                float attack = (float)counter.attackSum[i];
                float critical = (float)counter.criticalSum[i];

                attackRate[i] = attack / attackSum;
                criticalRate[i] = critical > 0 ? 1f - attackRate[i] : 0;
                categoryRate[i] = (attack + critical) / attackSum;
            }

            float shieldSum = (float)counter.shieldSum;
            float damageSum = (float)counter.damageSum;

            float shieldRate = shieldSum > 0 ? shieldSum / (damageSum + shieldSum) : 0f;
            float damageRate = damageSum > 0 ? 1f - shieldRate : 0f;

            int knuckle = (int)EquipmentCategory.Knuckle;
            int sword = (int)EquipmentCategory.Sword;

            titlePoint[Title.MinimumStep] = MinimumStep(counter.stepSum);
            titlePoint[Title.Zenigata] = Zenigata(counter.CoinSum);
            titlePoint[Title.FastRunner] = FastRunner(clearTimeSec);

            titlePoint[Title.Kuroobi] = Fighter(counter.attackSum[knuckle], attackSum, fighter, criticalRate[knuckle]);
            titlePoint[Title.SwordMaster] = Fighter(counter.attackSum[sword], attackSum, fighter, criticalRate[sword]);
            titlePoint[Title.ShieldMaster] = Fighter(counter.attackSum[(int)EquipmentCategory.Shield], attackSum, fighter, shieldRate);

            titlePoint[Title.PhantomThief] = PhantomThief(treasureComp);

            titlePoint[Title.Mapper] = mapComp;

            titlePoint[Title.DarkMagician] = Magician(counter.magicSum[(int)AttackAttr.Dark - 1], magic, magician);
            titlePoint[Title.FireMagician] = Magician(counter.magicSum[(int)AttackAttr.Fire - 1], magic, magician);
            titlePoint[Title.IceMagician] = Magician(counter.magicSum[(int)AttackAttr.Ice - 1], magic, magician);

            titlePoint[Title.Grappler] = Fighter(counter.attackSum[knuckle], attackSum, fighter, attackRate[knuckle]);
            titlePoint[Title.SwordFighter] = Fighter(counter.attackSum[sword], attackSum, fighter, attackRate[sword]);
            titlePoint[Title.Shielder] = Fighter(counter.attackSum[(int)EquipmentCategory.Shield], attackSum, fighter, damageRate);

            titlePoint[Title.Magician] = magician;
            titlePoint[Title.Fighter] = fighter;

            titlePoint[Title.SlowPoke] = SlowPoke(clearTimeSec);

            var title = titlePoint.OrderByDescending(kv => kv.Value).ToArray()[0].Key;
            var bonus = titleBonus[title];

            titleData = (title, bonus);
        }

        private float FastRunner(float clearTimeSec) => Mathf.Clamp01(2f - clearTimeSec / 1800f);
        private float SlowPoke(float clearTimeSec) => Mathf.Clamp01(clearTimeSec / 10800f);
        private float MinimumStep(float step) => Mathf.Clamp01(1f - step / 1000f);
        private float PhantomThief(float treasureComp) => treasureComp;
        private float Zenigata(int coin) => Mathf.Min(1f, (float)coin / 100f);
        private float Magician(int attr, float magicSum, float magician)
            => magicSum > 0 ? Mathf.Min(1f, magician * attr / magicSum * 3f) : 0f;

        private float Fighter(int category, float attackSum, float fighter, float specificRate)
            => attackSum > 0 ? Mathf.Min(1f, fighter * specificRate * category / attackSum * 6f) : 0f;
    }
}
