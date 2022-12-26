using System;
using UnityEngine;

[Serializable]
public class PlayerCounter
{
    [SerializeField] private int defeat = 0;
    [SerializeField] private int attack = 0;
    [SerializeField] private int shield = 0;
    [SerializeField] private int magic = 0;
    [SerializeField] private int damage = 0;
    [SerializeField] private int magicDamage = 0;

    public int Defeat => defeat;
    public int Attack => attack;
    public int Shield => shield;
    public int Magic => magic;
    public int Damage => damage;
    public int MagicDamage => magicDamage;

    [SerializeField] private int defeatSum = 0;
    [SerializeField] private int attackSum = 0;
    [SerializeField] private int shieldSum = 0;
    [SerializeField] private int magicSum = 0;
    [SerializeField] private int damageSum = 0;
    [SerializeField] private int magicDamageSum = 0;

    public int DefeatSum => defeatSum;
    public int AttackSum => attackSum;
    public int ShieldSum => shieldSum;
    public int MagicSum => magicSum;
    public int DamageSum => damageSum;
    public int MagicDamageSum => magicDamageSum;

    public int IncDefeat() => ++defeat;
    public int IncAttack() => ++attack;
    public int IncShield() => ++shield;
    public int IncMagic() => ++magic;
    public int IncDamage() => ++damage;
    public int DecDamage() => --damage;
    public int IncMagicDamage() => ++magicDamage;

    public void TotalCounts()
    {
        defeatSum += defeat;
        attackSum += attack;
        shieldSum += shield;
        magicSum += magic;
        damageSum += damage;
        magicDamageSum += magicDamage;

        defeat = attack = shield = magic = damage = magicDamage = 0;
    }
}
