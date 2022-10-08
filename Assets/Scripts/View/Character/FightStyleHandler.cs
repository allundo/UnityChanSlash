using UnityEngine;
using System.Collections.Generic;

public class FightStyleHandler : MonoBehaviour
{
    [SerializeField] protected MobAttack[] attacks = default;

    public IMobAttack Attack(int index) => attacks[index];

    public IEnumerable<IMobAttack> Attacks => attacks;
}