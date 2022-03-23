using UnityEngine;

[RequireComponent(typeof(WitchAnimator))]
[RequireComponent(typeof(WitchReactor))]
[RequireComponent(typeof(MagicAndDouble))]
public class WitchAIInput : GhostAIInput, IUndeadInput
{
    protected ICommand sleep;
    protected ICommand backAttack;
    protected ICommand targetAttack;
    protected ICommand backStep;
    protected ICommand ice;
    protected ICommand magic;
    protected ICommand teleport;

    protected override void SetCommands()
    {
        var enemyTarget = target as EnemyCommandTarget;

        var doubleAttack = new WitchDoubleAttackLaunch(enemyTarget, 32f);
        attack = new WitchBackStepAttack(enemyTarget, 32f, doubleAttack);
        backAttack = new WitchJumpOverAttack(enemyTarget, 32f, doubleAttack);

        die = new EnemyDie(enemyTarget, 72f);
        targetAttack = new WitchTargetAttack(enemyTarget, 75f);
        moveForward = new GhostForward(enemyTarget, 48f);
        backStep = new WitchBackStep(enemyTarget, 32f);
        throughForward = new GhostThrough(enemyTarget, 48f, new GhostAttackStart(enemyTarget, 16f));
        turnL = new EnemyTurnAnimL(enemyTarget, 8f);
        turnR = new EnemyTurnAnimR(enemyTarget, 8f);

        fire = new WitchTripleFire(enemyTarget, 72f);
        ice = new WitchDoubleIce(enemyTarget, 72f);
        magic = new WitchMagic(enemyTarget, 108f);
        teleport = new WitchTeleport(enemyTarget, 84f);

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

        Pos forward = map.GetForward;

        // Start attack if player found at forward
        if (IsOnPlayer(forward)) return RandomChoice(attack, backAttack, targetAttack, backStep);

        // Turn if player found at left, right or backward
        Pos left = map.GetLeft;
        if (IsOnPlayer(left)) return turnL;

        Pos right = map.GetRight;
        if (IsOnPlayer(right)) return turnR;

        Pos left2 = map.dir.GetLeft(left);
        if (IsOnPlayer(left2)) return turnL;

        Pos right2 = map.dir.GetRight(right);
        if (IsOnPlayer(right2)) return turnR;

        Pos forward2 = map.dir.GetForward(forward);
        bool isForwardMovable = map.IsMovable(forward);

        // Move forward if player found in front
        if (IsOnPlayer(forward2))
        {
            return isForwardMovable ? RandomChoice(moveForward, fire, ice, magic) : throughForward;
        }

        Pos forward3 = map.dir.GetForward(forward2);
        if (IsOnPlayer(forward3) && isForwardMovable) return RandomChoice(moveForward, fire, ice);

        Pos backward = map.GetBackward;
        if (IsOnPlayer(backward) && Random.Range(0, 3) == 0) return RandomChoice(turnL, turnR);

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
            if (map.GetTile(forward2).IsViewOpen) return RandomChoice(throughForward, teleport);

            // Turn if forward unmovable and left or right movable
            if (map.IsMovable(left)) return turnL;
            if (map.IsMovable(right)) return turnR;
            if (map.IsMovable(backward)) return RandomChoice(turnL, turnR);
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
