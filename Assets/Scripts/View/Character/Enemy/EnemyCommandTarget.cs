using UnityEngine;

[RequireComponent(typeof(EnemyReactor))]
[RequireComponent(typeof(EnemyMapUtil))]
[RequireComponent(typeof(MobAnimator))]
public class EnemyCommandTarget : CommandTarget
{
    /// <summary>
    /// Enemy attack handler for EnemyCommand execution.
    /// </summary>
    [SerializeField] public MobAttack[] enemyAttack = default;
}
