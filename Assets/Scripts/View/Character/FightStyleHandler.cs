using UnityEngine;
using System.Collections.Generic;

public class FightStyleHandler : MonoBehaviour
{
    [SerializeField] protected MobAttack[] attacks = default;
    [SerializeField] protected float shieldRatioR = 0f;
    [SerializeField] protected float shieldRatioL = 1f;

    public float ShieldRatioR => shieldRatioR;
    public float ShieldRatioL => shieldRatioL;

    public IMobAttack Attack(int index) => attacks[index];

    public IEnumerable<IMobAttack> Attacks => attacks;
}