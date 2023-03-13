using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class PlayerCounter
{
    [SerializeField] private int[] defeat = new int[Util.Count<EnemyType>() - 1];
    [SerializeField] private int attack = 0;
    [SerializeField] private int shield = 0;
    [SerializeField] private int magic = 0;
    [SerializeField] private int damage = 0;
    [SerializeField] private int magicDamage = 0;

    public int Defeat => defeat.Sum();
    public int Attack => attack;
    public int Shield => shield;
    public int Magic => magic;
    public int Damage => damage;
    public int MagicDamage => magicDamage;

    [SerializeField] private int[] defeatSum = new int[Util.Count<EnemyType>() - 1];
    [SerializeField] private int attackSum = 0;
    [SerializeField] private int shieldSum = 0;
    [SerializeField] private int magicSum = 0;
    [SerializeField] private int damageSum = 0;
    [SerializeField] private int magicDamageSum = 0;
    [SerializeField] private int coinSum = 0;
    [SerializeField] private int potionSum = 0;

    public int DefeatSum => defeatSum.Sum();
    public int DefeatType(EnemyType type) => defeatSum[(int)type - 1];
    public int AttackSum => attackSum;
    public int ShieldSum => shieldSum;
    public int MagicSum => magicSum;
    public int DamageSum => damageSum;
    public int MagicDamageSum => magicDamageSum;
    public int CoinSum => coinSum;
    public int PotionSum => potionSum;

    public void IncDefeat(EnemyType type = EnemyType.None)
    {
        if (type != EnemyType.None) ++defeat[(int)type - 1];
    }

    public void IncAttack() => ++attack;
    public void IncShield() => ++shield;
    public void IncMagic() => ++magic;
    public void IncDamage() => ++damage;
    public void DecDamage() => --damage;
    public void IncMagicDamage() => ++magicDamage;
    public void IncCoin() => ++coinSum;
    public void IncPotion() => ++potionSum;

    public void TotalCounts()
    {
        for (int i = 0; i < defeat.Length; i++)
        {
            defeatSum[i] += defeat[i];
            defeat[i] = 0;
        }

        attackSum += attack;
        shieldSum += shield;
        magicSum += magic;
        damageSum += damage;
        magicDamageSum += magicDamage;

        attack = shield = magic = damage = magicDamage = 0;
    }
}
