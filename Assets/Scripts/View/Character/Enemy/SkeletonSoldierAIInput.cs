using UnityEngine;

[RequireComponent(typeof(SkeletonSoldierAnimator))]
[RequireComponent(typeof(SkeletonSoldierReactor))]
public class SkeletonSoldierAIInput : GoblinAIInput, IUndeadInput
{
    protected ICommand sleep;

    protected override void SetCommands()
    {
        die = new EnemyDie(target, 72f);
        idle = new EnemyIdle(target, 36f);
        moveForward = new EnemyForward(target, 72f);
        run = new EnemyForward(target, 36f);
        turnL = new ShieldEnemyTurnL(target, 16f);
        turnR = new ShieldEnemyTurnR(target, 16f);
        guard = new GuardCommand(target, 36f, 0.95f);
        attack = new EnemyAttack(target, 86f);

        sleep = new UndeadSleep(target, 300f, new Resurrection(target, 64f));
    }

    protected override void SetInputs()
    {
        guardState = new GuardState(this, 19f);
        shieldAnim = target.anim as SkeletonSoldierAnimator;
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
        Pos forward = mobMap.GetForward;
        shieldAnim.fighting.Bool = IsOnPlayer(forward);
        shieldAnim.guard.Bool &= shieldAnim.fighting.Bool;

        // Attack or Guard if fighting
        if (shieldAnim.fighting.Bool)
        {
            if (currentCommand is EnemyIdle) return attack;
            return RandomChoice(attack, guard, idle);
        }

        Pos forward2 = mobMap.dir.GetForward(forward);
        bool isForwardMovable = mobMap.IsMovable(forward);

        // Run forward if player found in front
        if (IsOnPlayer(forward2) && isForwardMovable) return run;

        // Turn if player found at left, right or backward
        Pos left = mobMap.GetLeft;
        if (IsOnPlayer(left)) return turnL;

        Pos right = mobMap.GetRight;
        if (IsOnPlayer(right)) return turnR;

        Pos backward = mobMap.GetBackward;
        if (IsOnPlayer(backward)) return RandomChoice(turnL, turnR);

        Pos left2 = mobMap.dir.GetLeft(left);
        bool isLeftMovable = mobMap.IsMovable(left);
        if (IsOnPlayer(left2) && isLeftMovable) return turnL;

        Pos right2 = mobMap.dir.GetRight(right);
        bool isRightMovable = mobMap.IsMovable(right);
        if (IsOnPlayer(right2) && isRightMovable) return turnR;

        Pos backward2 = mobMap.dir.GetBackward(backward);
        bool isBackwardMovable = mobMap.IsMovable(backward);
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
