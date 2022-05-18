using UnityEngine;

[RequireComponent(typeof(FlyingAnimator))]
[RequireComponent(typeof(GhostReactor))]
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
            if (mobMap.GetTile(forward2).IsViewOpen) return throughForward;

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
