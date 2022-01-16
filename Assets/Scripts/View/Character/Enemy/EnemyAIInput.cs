using UnityEngine;

public class EnemyAIInput : MobInput
{
    protected ICommand turnL;
    protected ICommand turnR;
    protected ICommand moveForward;
    protected ICommand attack;
    protected ICommand fire;

    protected bool IsOnPlayer(Pos pos) => MapUtil.IsOnPlayer(pos);

    protected override void SetCommands()
    {
        var enemyTarget = target as EnemyCommandTarget;

        die = new EnemyDie(enemyTarget, 72f);
        moveForward = new EnemyForward(enemyTarget, 72f);
        turnL = new EnemyTurnL(enemyTarget, 8f);
        turnR = new EnemyTurnR(enemyTarget, 8f);
        attack = new EnemyAttack(enemyTarget, 72f);
        fire = new EnemyFire(enemyTarget, 108f);
    }

    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        // Turn if player found at left, right or backward
        Pos left = map.GetLeft;
        Pos right = map.GetRight;

        if (IsOnPlayer(left)) return turnL;
        if (IsOnPlayer(right)) return turnR;

        Pos backward = map.GetBackward;

        if (IsOnPlayer(backward))
        {
            return Random.Range(0, 2) == 0 ? turnL : turnR;
        }

        // Attack if player found at forward
        Pos forward = map.GetForward;
        if (IsOnPlayer(forward)) return attack;

        // Move forward if player found in front
        if (map.IsPlayerFound(forward)) return moveForward;

        bool isForwardMovable = map.IsMovable(forward);
        bool isLeftMovable = map.IsMovable(left);
        bool isRightMovable = map.IsMovable(right);

        if (isForwardMovable)
        {
            // Turn 50% if left or right movable
            if (Random.Range(0, 2) == 0)
            {
                if (Random.Range(0, 2) == 0)
                {
                    if (currentCommand == turnR) return moveForward;
                    if (isLeftMovable) return turnL;
                    if (isRightMovable) return turnR;
                }
                else
                {
                    if (currentCommand == turnL) return moveForward;
                    if (isRightMovable) return turnR;
                    if (isLeftMovable) return turnL;
                }
            }

            // Move forward if not turned and forward movable
            return moveForward;
        }
        else
        {
            // Turn if forward unmovable and left or right movable
            if (isLeftMovable) return turnL;
            if (isRightMovable) return turnR;

            // Turn if backward movable
            if (map.IsMovable(backward))
            {
                return Random.Range(0, 2) == 0 ? turnL : turnR;
            }
        }

        // Idle if unmovable
        return null;
    }
}
