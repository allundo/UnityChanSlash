using UnityEngine;

[RequireComponent(typeof(Magic))]
public class RedSlimeAIInput : EnemyAIInput
{
    protected override void SetCommands()
    {
        die = new EnemyDie(target, 72f);
        idle = new EnemyIdle(target, 60f);
        moveForward = new EnemyForward(target, 96f);
        turnL = new EnemyTurnL(target, 20f);
        turnR = new EnemyTurnR(target, 20f);
        attack = new EnemyAttack(target, 100f);
        fire = new EnemyFire(target, 108f, MagicType.FireBall);
    }
    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        Pos forward = mobMap.GetForward;
        Pos backward = mobMap.GetBackward;
        Pos left = mobMap.GetLeft;
        Pos right = mobMap.GetRight;

        bool isBackwardMovable = mobMap.IsMovable(backward);
        bool isLeftMovable = mobMap.IsMovable(left);
        bool isRightMovable = mobMap.IsMovable(right);

        // Get away if player found at forward, or attack if not movable.
        if (IsOnPlayer(forward)) return choice.TurnToMovable(isLeftMovable, isRightMovable, isBackwardMovable) ?? attack;

        // Get away if player found at backward or player found at 2 tiles backward even when fleeing
        bool isOnPlayerBackward = IsOnPlayer(backward);
        Pos backward2 = mobMap.dir.GetBackward(backward);
        bool isOnPlayerBackward2 = IsOnPlayer(backward2);
        bool isForwardMovable = mobMap.IsMovable(forward);
        if (isOnPlayerBackward || isOnPlayerBackward2 && currentCommand == moveForward)
        {
            if (isForwardMovable) return moveForward;
            return choice.TurnToMovable(isLeftMovable, isRightMovable, true);
        }

        // Get away if player found at left or right
        if (IsOnPlayer(left))
        {
            if (isRightMovable) return turnR;
            if (isForwardMovable) return moveForward;
            if (isBackwardMovable) return turnR;

            // Turn to player if cannot get away
            return turnL;
        }

        if (IsOnPlayer(right))
        {
            if (isLeftMovable) return turnL;
            if (isForwardMovable) return moveForward;
            if (isBackwardMovable) return turnL;

            // Turn to player if cannot get away
            return turnR;
        }

        // Shoot if player found in front
        bool isForwardOpen = mobMap.GetTile(forward).IsViewOpen;
        if (IsPlayerFound(forward) && isForwardOpen) return fire;

        // Turn to player if player is found in 2 or 3 tile distance
        Pos left2 = mobMap.dir.GetLeft(left);
        Pos left3 = mobMap.dir.GetLeft(left2);
        bool isLeftViewOpen = IsViewOpen(left);
        if (isLeftViewOpen && IsOnPlayer(left2) || IsViewOpen(left2) && IsOnPlayer(left3)) return turnL;

        Pos right2 = mobMap.dir.GetRight(right);
        Pos right3 = mobMap.dir.GetLeft(right2);
        bool isRightViewOpen = IsViewOpen(right);
        if (isRightViewOpen && IsOnPlayer(right2) || IsViewOpen(right2) && IsOnPlayer(right3)) return turnR;

        Pos backward3 = mobMap.dir.GetBackward(backward2);
        bool isBackwardViewOpen = IsViewOpen(backward);
        if (isBackwardViewOpen && isOnPlayerBackward2 || IsViewOpen(backward2) && IsOnPlayer(backward3))
        {
            ICommand choice = RandomChoice(turnL, turnR);
            if (choice == turnL && !isLeftMovable) return turnR;
            if (choice == turnR && !isRightMovable) return turnL;
            return choice;
        }

        return choice.MoveForwardOrTurn(isForwardMovable, isLeftMovable, isRightMovable, isBackwardMovable || IsOnPlayer(backward))
            ?? choice.TurnToViewOpen(IsViewOpen(forward), isLeftViewOpen, isRightViewOpen, isBackwardViewOpen);
    }
}
