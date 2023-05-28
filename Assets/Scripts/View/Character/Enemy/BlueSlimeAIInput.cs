using UnityEngine;

[RequireComponent(typeof(Magic))]
public class BlueSlimeAIInput : EnemyAIInput
{
    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        // Turn if player found at left, right or backward
        Pos left = mobMap.GetLeft;
        Pos right = mobMap.GetRight;

        if (IsOnPlayer(left)) return turnL;
        if (IsOnPlayer(right)) return turnR;

        Pos backward = mobMap.GetBackward;

        if (IsOnPlayer(backward))
        {
            return RandomChoice(turnL, turnR);
        }

        // Attack or ice magic if player found at forward
        bool isPlayerIcing = PlayerInfo.Instance.IsPlayerIcing;
        Pos forward = mobMap.GetForward;
        if (IsOnPlayer(forward)) return isPlayerIcing ? attack : RandomChoice(attack, fire);

        // Shoot if player found in front
        bool isForwardMovable = mobMap.IsMovable(forward);
        if (IsPlayerFound()) return isForwardMovable && isPlayerIcing ? moveForward : fire;

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

        return MoveForwardOrTurn(isForwardMovable, mobMap.IsMovable(left), mobMap.IsMovable(right), mobMap.IsMovable(backward))
            ?? TurnToViewOpen(IsViewOpen(forward), isLeftViewOpen, isRightViewOpen, isBackwardViewOpen);
    }
}
