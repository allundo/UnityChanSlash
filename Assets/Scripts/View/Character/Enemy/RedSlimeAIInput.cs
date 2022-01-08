using UnityEngine;

[RequireComponent(typeof(Fire))]
public class RedSlimeAIInput : EnemyAIInput
{
    protected override Command GetCommand()
    {
        var currentCommand = commander.currentCommand;

        Pos forward = map.GetForward;
        Pos backward = map.GetBackward;
        Pos left = map.GetLeft;
        Pos right = map.GetRight;

        bool isForwardMovable = map.IsMovable(forward);
        bool isBackwardMovable = map.IsMovable(backward);
        bool isLeftMovable = map.IsMovable(left);
        bool isRightMovable = map.IsMovable(right);

        // Get away if player found at forward
        if (IsOnPlayer(forward))
        {
            if (isBackwardMovable || isLeftMovable && isRightMovable)
            {
                return Random.Range(0, 2) == 0 ? turnL : turnR;
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
        bool isForwardOpen = map.GetTile(forward).IsViewOpen;
        if (map.IsPlayerFound(forward) && isForwardOpen) return fire;

        Pos backward2 = map.dir.GetBackward(backward);
        Pos left2 = map.dir.GetLeft(left);
        Pos right2 = map.dir.GetRight(right);

        // Turn to player if player is found in 2 tile distance
        if (IsOnPlayer(backward2)) return Random.Range(0, 2) == 0 ? turnL : turnR;
        if (IsOnPlayer(left2)) return turnL;
        if (IsOnPlayer(right2)) return turnR;

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
            if (isLeftMovable && !isRightMovable) return turnL;
            if (isRightMovable && !isLeftMovable) return turnR;

            return Random.Range(0, 2) == 0 ? turnL : turnR;
        }
    }
}