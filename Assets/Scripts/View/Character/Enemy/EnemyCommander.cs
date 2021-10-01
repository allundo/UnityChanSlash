using UnityEngine;

[RequireComponent(typeof(EnemyAnimator))]
public class EnemyCommander : MobCommander
{
    [SerializeField] public MobAttack enemyAttack = default;
}
