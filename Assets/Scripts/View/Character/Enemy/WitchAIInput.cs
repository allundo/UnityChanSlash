using UnityEngine;

[RequireComponent(typeof(WitchAnimator))]
[RequireComponent(typeof(WitchReactor))]
[RequireComponent(typeof(MagicAndDouble))]
public class WitchAIInput : GhostAIInput, IUndeadInput
{
    protected ICommand jumpOverAttack;
    protected ICommand targetAttack;
    protected ICommand backStep;
    protected ICommand ice;
    protected ICommand laser;
    protected ICommand summon;
    protected ICommand teleport;

    protected UndeadInput undeadInput;

    protected override void SetCommands()
    {
        var doubleAttack = new WitchDoubleAttackLaunch(target, 32f);
        attack = new WitchBackStepAttack(target, 32f, doubleAttack);
        jumpOverAttack = new WitchJumpOverAttack(target, 32f, doubleAttack);

        die = new EnemyDie(target, 72f);
        targetAttack = new WitchTargetAttack(target, 75f);
        moveForward = new GhostForward(target, 48f);
        backStep = new WitchBackStep(target, 32f);
        throughForward = new GhostThrough(target, 48f, new GhostAttackStart(target, 16f));
        turnL = new EnemyTurnAnimL(target, 8f);
        turnR = new EnemyTurnAnimR(target, 8f);

        fire = new WitchTripleFire(target, 72f);
        ice = new WitchDoubleIce(target, 72f);
        laser = new WitchLaser(target, 380f);
        summon = new WitchSummonMonster(target, 108f);
        teleport = new MagicianTeleport(target, 84f);

        undeadInput = new UndeadInput(cmd => Interrupt(cmd, true, true), new WitchSleep(target), new WitchQuickSleep(target));
    }

    public void InterruptSleep() => undeadInput.InterruptSleep();
    public override void OnActive(EnemyStatus.ActivateOption option)
    {
        base.OnActive(option);
        undeadInput.OnActive(option);
    }

    protected bool IsClosedDoor(Pos pos)
    {
        Door door = mobMap.GetTile(pos) as Door;
        return door != null && !door.IsOpen;
    }

    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        Pos forward = mobMap.GetForward;
        Pos backward = mobMap.GetBackward;
        Pos forward2 = mobMap.dir.GetForward(forward);

        bool isBackwardMovable = mobMap.IsMovable(backward);
        bool isForward2Movable = mobMap.IsMovable(forward2);
        bool isLeapable = isForward2Movable && mobMap.IsLeapable(forward);

        // Start attack if player found at forward
        if (IsOnPlayer(forward))
        {
            ICommand cmd = RandomChoice(attack, jumpOverAttack, targetAttack, backStep, teleport);

            if (cmd == targetAttack || cmd == teleport) return cmd;
            if ((cmd == attack || cmd == backStep) && !isBackwardMovable && isLeapable) return jumpOverAttack;
            if (cmd == jumpOverAttack && !isLeapable && isBackwardMovable) return RandomChoice(attack, backStep);
            return RandomChoice(fire, ice, laser);
        }

        // Turn if player found at left, right or backward
        Pos left = mobMap.GetLeft;
        if (IsOnPlayer(left)) return turnL;

        Pos right = mobMap.GetRight;
        if (IsOnPlayer(right)) return turnR;

        Pos left2 = mobMap.dir.GetLeft(left);
        if (IsOnPlayer(left2)) return turnL;

        Pos right2 = mobMap.dir.GetRight(right);
        if (IsOnPlayer(right2)) return turnR;

        bool isForwardMovable = mobMap.IsMovable(forward);
        bool isForwardDoor = IsClosedDoor(forward);

        // Move forward if player found in front
        if (IsOnPlayer(forward2))
        {
            if (isForwardDoor) return laser;
            if (!isForwardMovable) return throughForward;
            return RandomChoice(
                currentCommand == backStep ? ice : moveForward,
                currentCommand == moveForward ? fire : backStep,
                fire,
                ice,
                summon,
                laser
            );
        }

        Pos forward3 = mobMap.dir.GetForward(forward2);

        if (IsOnPlayer(forward3))
        {
            if (isForwardDoor) return laser;

            if (isForwardMovable)
            {
                if (IsClosedDoor(forward2)) return laser;
                return RandomChoice(moveForward, fire, ice, laser);
            }
        }

        if (IsOnPlayer(backward) && Util.Judge(3)) return RandomChoice(turnL, turnR);

        bool isLeftMovable = mobMap.IsMovable(left);
        bool isRightMovable = mobMap.IsMovable(right);

        return MoveForwardOrTurn(isForwardMovable, isLeftMovable, isRightMovable)
            ?? ThroughWall(isForward2Movable)
            ?? TurnToMovable(isForwardMovable, isLeftMovable, isRightMovable, isBackwardMovable)
            ?? idle;
    }
    protected override ICommand ThroughWall(bool isForward2Movable)
    {
        return isForward2Movable ? RandomChoice(throughForward, teleport) : null;
    }

    public void InputTeleport() => ForceEnqueue(teleport);
}
