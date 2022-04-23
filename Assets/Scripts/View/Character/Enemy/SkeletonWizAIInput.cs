using UnityEngine;

[RequireComponent(typeof(SkeletonWizAnimator))]
[RequireComponent(typeof(SkeletonWizReactor))]
public class SkeletonWizAIInput : EnemyAIInput, IUndeadInput
{
    protected ICommand sleep;
    protected ICommand ice;
    protected ICommand teleport;

    protected override void SetCommands()
    {
        var enemyTarget = target as EnemyCommandTarget;

        attack = new EnemyAttack(enemyTarget, 64f);

        die = new EnemyDie(enemyTarget, 72f);
        moveForward = new EnemyForward(enemyTarget, 64f);
        turnL = new EnemyTurnAnimL(enemyTarget, 16f);
        turnR = new EnemyTurnAnimR(enemyTarget, 16f);

        fire = new EnemyFire(enemyTarget, 72f, BulletType.DarkHound);
        ice = new EnemyFire(enemyTarget, 72f, BulletType.IceBullet);
        teleport = new MagicianTeleport(enemyTarget, 84f, 3);

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

        Pos forward = mobMap.GetForward;

        // Start attack if player found at forward
        if (IsOnPlayer(forward)) return RandomChoice(attack, teleport);

        // Turn if player found at left, right or backward
        Pos left = mobMap.GetLeft;
        if (IsOnPlayer(left)) return turnL;

        Pos right = mobMap.GetRight;
        if (IsOnPlayer(right)) return turnR;

        Pos left2 = mobMap.dir.GetLeft(left);
        if (IsOnPlayer(left2)) return turnL;

        Pos right2 = mobMap.dir.GetRight(right);
        if (IsOnPlayer(right2)) return turnR;

        if (mobMap.IsPlayerFound(forward)) return RandomChoice(ice, fire);

        Pos backward = mobMap.GetBackward;
        if (IsOnPlayer(backward)) return RandomChoice(turnL, turnR);

        bool isForwardMovable = mobMap.IsMovable(forward);

        if (isForwardMovable)
        {
            // Turn 50%
            if (Random.Range(0, 2) == 0)
            {
                if (Random.Range(0, 2) == 0)
                {
                    return (currentCommand == turnR) ? moveForward : turnL;
                }
                else
                {
                    return (currentCommand == turnL) ? moveForward : turnR;
                }
            }

            // Move forward if not turned and forward movable
            return moveForward;
        }
        else
        {
            // Turn if forward unmovable and left or right movable
            if (mobMap.IsMovable(left)) return turnL;
            if (mobMap.IsMovable(right)) return turnR;
            if (mobMap.IsMovable(backward)) return RandomChoice(turnL, turnR);
        }

        // Idle if unmovable
        return null;
    }

    // Doesn't fall by ice
    public override void InputIced(float duration)
    {
        // Delete Command queue only
        ClearAll(true);
        ICommand continuation = commander.PostponeCurrent();
        InputCommand(new IcedCommand(target, duration));
        if (continuation != null) ForceEnqueue(continuation);
    }
}
