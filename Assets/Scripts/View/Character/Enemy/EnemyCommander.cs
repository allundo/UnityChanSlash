using UnityEngine;

[RequireComponent(typeof(EnemyAnimator))]
public class EnemyCommander : MobCommander
{
    /// <summary>
    /// Enemy attack handler for EnemyCommand execution.
    /// </summary>
    [SerializeField] public MobAttack enemyAttack = default;
}
