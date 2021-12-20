using UnityEngine;

[RequireComponent(typeof(EnemyReactor))]
[RequireComponent(typeof(EnemyMapUtil))]
public class EnemyCommandTarget : CommandTarget
{
    /// <summary>
    /// Enemy attack handler for EnemyCommand execution.
    /// </summary>
    [SerializeField] public MobAttack enemyAttack = default;
    [SerializeField] public Fire enemyFire = default;
}
