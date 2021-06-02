using UnityEngine;

[RequireComponent(typeof(EnemyAnimator))]
public partial class EnemyCommander : MobCommander
{
    public EnemyAnimator enemyAnim { get; protected set; }
    protected EnemyAI enemyAI;

    protected override void SetCommands()
    {
        die = new DieCommand(this, 3.0f);
        enemyAI = new EnemyAI(this);
    }

    protected override Command GetCommand()
    {
        return enemyAI.GetCommand();
    }
}
