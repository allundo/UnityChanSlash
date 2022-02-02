using UnityEngine;

[RequireComponent(typeof(SkeletonSoldierAnimator))]
[RequireComponent(typeof(SkeletonSoldierReactor))]
public class SkeletonSoldierAIInput : GoblinAIInput, IUndeadInput
{
    protected ICommand sleep;

    protected override void SetCommands()
    {
        var enemyTarget = target as EnemyCommandTarget;

        die = new EnemyDie(enemyTarget, 72f);
        idle = new EnemyIdle(enemyTarget, 36f);
        moveForward = new EnemyForward(enemyTarget, 72f);
        run = new EnemyForward(enemyTarget, 36f);
        turnL = new ShieldEnemyTurnL(enemyTarget, 16f);
        turnR = new ShieldEnemyTurnR(enemyTarget, 16f);
        guard = new GuardCommand(enemyTarget, 36f, 0.95f);
        attack = new EnemyAttack(enemyTarget, 86f);

        sleep = new UndeadSleep(enemyTarget, 300f, new Resurrection(enemyTarget, 64f));
    }

    public void InputSleep()
    {
        ClearAll();
        Interrupt(sleep);
    }

    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        // Fighting start if player found at forward
        Pos forward = map.GetForward;
        shieldAnim.fighting.Bool = IsOnPlayer(forward);
        shieldAnim.guard.Bool &= shieldAnim.fighting.Bool;

        // Attack or Guard if fighting
        if (shieldAnim.fighting.Bool)
        {
            if (currentCommand is EnemyIdle) return attack;
            return RandomChoice(attack, guard, idle);
        }

        Pos forward2 = map.dir.GetForward(forward);
        bool isForwardMovable = map.IsMovable(forward);

        // Run forward if player found in front
        if (IsOnPlayer(forward2) && isForwardMovable) return run;

        // Turn if player found at left, right or backward
        Pos left = map.GetLeft;
        if (IsOnPlayer(left)) return turnL;

        Pos right = map.GetRight;
        if (IsOnPlayer(right)) return turnR;

        Pos backward = map.GetBackward;
        if (IsOnPlayer(backward)) return RandomChoice(turnL, turnR);

        Pos left2 = map.dir.GetLeft(left);
        bool isLeftMovable = map.IsMovable(left);
        if (IsOnPlayer(left2) && isLeftMovable) return turnL;

        Pos right2 = map.dir.GetRight(right);
        bool isRightMovable = map.IsMovable(right);
        if (IsOnPlayer(right2) && isRightMovable) return turnR;

        Pos backward2 = map.dir.GetBackward(backward);
        bool isBackwardMovable = map.IsMovable(backward);
        if (IsOnPlayer(backward2) && isBackwardMovable) return RandomChoice(turnL, turnR);

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
            if (isBackwardMovable)
            {
                return RandomChoice(turnL, turnR);
            }
        }

        // Idle if unmovable
        return null;
    }
}
