using UnityEngine;

[RequireComponent(typeof(Magic))]
public class RedSlimeAIInput : EnemyAIInput
{
    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        Pos forward = mobMap.GetForward;
        Pos backward = mobMap.GetBackward;
        Pos left = mobMap.GetLeft;
        Pos right = mobMap.GetRight;

        bool isForwardMovable = mobMap.IsMovable(forward);
        bool isBackwardMovable = mobMap.IsMovable(backward);
        bool isLeftMovable = mobMap.IsMovable(left);
        bool isRightMovable = mobMap.IsMovable(right);

        // Get away if player found at forward
        if (IsOnPlayer(forward))
        {
            if (isBackwardMovable || isLeftMovable && isRightMovable)
            {
                return RandomChoice(turnL, turnR);
            }

            if (isLeftMovable) return turnL;
            if (isRightMovable) return turnR;

            // Attack if cannot get away
            return attack;
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

        Pos backward2 = mobMap.dir.GetBackward(backward);
        Pos left2 = mobMap.dir.GetLeft(left);
        Pos right2 = mobMap.dir.GetRight(right);

        // Turn to player if player is found in 2 tile distance
        bool isBackwardViewOpen = IsViewOpen(backward);
        if (IsOnPlayer(backward2) && isBackwardViewOpen) return RandomChoice(turnL, turnR);
        bool isLeftViewOpen = IsViewOpen(left);
        if (IsOnPlayer(left2) && isLeftViewOpen) return turnL;
        bool isRightViewOpen = IsViewOpen(right);
        if (IsOnPlayer(right2) && isRightViewOpen) return turnR;

        return MoveForwardOrTurn(isForwardMovable, isLeftMovable, isRightMovable, isBackwardMovable || IsOnPlayer(backward))
            ?? TurnToViewOpen(IsViewOpen(forward), isLeftViewOpen, isRightViewOpen, isBackwardViewOpen);
    }

}
