using UnityEngine;

[RequireComponent(typeof(EnemyAnimator))]
public class EnemyCommandTarget : CommandTarget
{
    /// <summary>
    /// Enemy attack handler for EnemyCommand execution.
    /// </summary>
    [SerializeField] public MobAttack enemyAttack = default;
}
