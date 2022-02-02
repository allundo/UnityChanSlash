using UnityEngine;

[RequireComponent(typeof(SkeletonSoldierAnimator))]
[RequireComponent(typeof(SkeletonSoldierReactor))]
public class SkeletonSoldierAIInput : GoblinAIInput, IUndeadInput
{
    protected ICommand sleep;

    protected override void SetCommands()
    {
        var enemyTarget = target as EnemyCommandTarget;
        sleep = new UndeadSleep(enemyTarget, 300f, new Resurrection(enemyTarget, 64f));

        base.SetCommands();
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

        // Attack or Guard if fighting
        if (shieldAnim.fighting.Bool)
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    return currentCommand is EnemyIdle ? attack : idle;
                case 1:
                    return guard;
                default:
                    return idle;
            }
        }

        // Move forward if player found in front
        if (map.IsPlayerFound(forward)) return run;

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
