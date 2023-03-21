using UnityEngine;

[RequireComponent(typeof(WitchAnimator))]
[RequireComponent(typeof(WitchReactor))]
[RequireComponent(typeof(MagicAndDouble))]
public class WitchAIInput : GhostAIInput, IUndeadInput
{
    protected ICommand backAttack;
    protected ICommand targetAttack;
    protected ICommand backStep;
    protected ICommand ice;
    protected ICommand magic;
    protected ICommand summon;
    protected ICommand teleport;

    protected UndeadInput undeadInput;

    protected override void SetCommands()
    {
        var doubleAttack = new WitchDoubleAttackLaunch(target, 32f);
        attack = new WitchBackStepAttack(target, 32f, doubleAttack);
        backAttack = new WitchJumpOverAttack(target, 32f, doubleAttack);

        die = new EnemyDie(target, 72f);
        targetAttack = new WitchTargetAttack(target, 75f);
        moveForward = new GhostForward(target, 48f);
        backStep = new WitchBackStep(target, 32f);
        throughForward = new GhostThrough(target, 48f, new GhostAttackStart(target, 16f));
        turnL = new EnemyTurnAnimL(target, 8f);
        turnR = new EnemyTurnAnimR(target, 8f);

        fire = new WitchTripleFire(target, 72f);
        ice = new WitchDoubleIce(target, 72f);
        magic = new WitchMagic(target, 108f);
        summon = new WitchSummonMonster(target, 108f);
        teleport = new MagicianTeleport(target, 84f);

        undeadInput = new UndeadInput(target, cmd => Interrupt(cmd, true, true));
    }

    public void InterruptSleep() => undeadInput.InterruptSleep();
    public override void OnActive(EnemyStatus.ActivateOption option)
    {
        base.OnActive(option);
        undeadInput.OnActive(option);
    }

    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        Pos forward = mobMap.GetForward;

        // Start attack if player found at forward
        if (IsOnPlayer(forward)) return RandomChoice(attack, backAttack, targetAttack, backStep);

        // Turn if player found at left, right or backward
        Pos left = mobMap.GetLeft;
        if (IsOnPlayer(left)) return turnL;

        Pos right = mobMap.GetRight;
        if (IsOnPlayer(right)) return turnR;

        Pos left2 = mobMap.dir.GetLeft(left);
        if (IsOnPlayer(left2)) return turnL;

        Pos right2 = mobMap.dir.GetRight(right);
        if (IsOnPlayer(right2)) return turnR;

        Pos forward2 = mobMap.dir.GetForward(forward);
        bool isForwardMovable = mobMap.IsMovable(forward) || mobMap.IsLeapable(forward);

        // Move forward if player found in front
        if (IsOnPlayer(forward2))
        {
            return isForwardMovable ? RandomChoice(moveForward, fire, ice, summon) : throughForward;
        }

        Pos forward3 = mobMap.dir.GetForward(forward2);
        if (IsOnPlayer(forward3) && isForwardMovable) return RandomChoice(moveForward, fire, ice);

        Pos backward = mobMap.GetBackward;
        if (IsOnPlayer(backward) && Util.Judge(3)) return RandomChoice(turnL, turnR);

        if (isForwardMovable)
        {
            // Turn 50%
            if (Util.Judge(2))
            {
                if (Util.Judge(2))
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
            if (mobMap.GetTile(forward2).IsViewOpen) return RandomChoice(throughForward, teleport);

            // Turn if forward unmovable and left or right movable
            if (mobMap.IsMovable(left)) return turnL;
            if (mobMap.IsMovable(right)) return turnR;
            if (mobMap.IsMovable(backward)) return RandomChoice(turnL, turnR);
        }

        // Idle if unmovable
        return null;
    }

    public void InputTeleport() => ForceEnqueue(teleport);
}
