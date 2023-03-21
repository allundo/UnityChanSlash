using System;
using System.Linq;
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
}
