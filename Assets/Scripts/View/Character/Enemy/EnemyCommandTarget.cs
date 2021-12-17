using UnityEngine;

[RequireComponent(typeof(EnemyAnimator))]
[RequireComponent(typeof(EnemyReactor))]
[RequireComponent(typeof(EnemyAIInput))]
[RequireComponent(typeof(EnemyMapUtil))]
public class EnemyCommandTarget : CommandTarget
{
    /// <summary>
    /// Enemy attack handler for EnemyCommand execution.
    /// </summary>
    [SerializeField] public MobAttack enemyAttack = default;
    [SerializeField] public Fire enemyFire = default;
}
