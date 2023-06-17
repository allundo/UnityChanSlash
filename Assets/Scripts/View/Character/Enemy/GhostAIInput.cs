using UnityEngine;

[RequireComponent(typeof(FlyingAnimator))]
[RequireComponent(typeof(GhostReactor))]
[RequireComponent(typeof(GhostMapUtil))]
public class GhostAIInput : EnemyAIInput
{
    protected ICommand throughForward;

    protected override void SetCommands()
    {
        die = new EnemyDie(target, 72f);
        attack = new GhostAttackStart(target, 30f);
        moveForward = new GhostForward(target, 64f);
        throughForward = new GhostThrough(target, 64f, attack);
        turnL = new EnemyTurnAnimL(target, 16f);
        turnR = new EnemyTurnAnimR(target, 16f);
    }

    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        Pos forward = mobMap.GetForward;

        // Start attack if player found at forward
        if (IsOnPlayer(forward)) return attack;

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
        bool isForwardMovable = mobMap.IsMovable(forward);

        // Move forward if player found in front
        if (IsOnPlayer(forward2))
        {
            return isForwardMovable ? RandomChoice(moveForward, attack) : throughForward;
        }

        Pos forward3 = mobMap.dir.GetForward(forward2);
        if (IsOnPlayer(forward3) && isForwardMovable) return moveForward;

        Pos backward = mobMap.GetBackward;
        if (IsOnPlayer(backward) && Util.Judge(3)) return RandomChoice(turnL, turnR);

        bool isLeftMovable = mobMap.IsMovable(left);
        bool isRightMovable = mobMap.IsMovable(right);

        return MoveForwardOrTurn(isForwardMovable, isLeftMovable, isRightMovable)
            ?? ThroughWall(mobMap.IsMovable(forward2))
            ?? TurnToMovable(isForwardMovable, isLeftMovable, isRightMovable, mobMap.IsMovable(backward))
            ?? idle;
    }

    protected virtual ICommand ThroughWall(bool isForward2Movable)
    {
        return isForward2Movable ? throughForward : null;
    }
}
