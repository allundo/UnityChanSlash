using UnityEngine;

public class FightStyle : MonoBehaviour
{
    [SerializeField] protected AttackBehaviour[] attacks = default;

    public virtual IAttack Attack(int index) => attacks[index];
}