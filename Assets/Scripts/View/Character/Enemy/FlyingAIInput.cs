using UnityEngine;

[RequireComponent(typeof(FlyingAnimator))]
public class FlyingAIInput : EnemyAIInput
{
    protected ICommand wakeUp;
    protected override void SetCommands()
    {
        die = new FlyingDie(target, 118f);
        wakeUp = new FlyingWakeUp(target, 120f);
        moveForward = new FlyingForward(target, 45f);
        turnL = new EnemyTurnAnimL(target, 7f);
        turnR = new EnemyTurnAnimR(target, 7f);
        attack = new FlyingAttackStart(target, 21f);
    }

    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        Pos forward = map.GetForward;

        // Start attack if player found at forward
        if (IsOnPlayer(forward)) return attack;

        bool isForwardMovable = map.ForwardTile.IsViewOpen;

        // Move forward if player found in front
        if (map.IsPlayerFound(forward) && isForwardMovable) return moveForward;

        // Turn if player found at left, right or backward
        Pos left = map.GetLeft;
        if (IsOnPlayer(left)) return turnL;

        Pos right = map.GetRight;
        if (IsOnPlayer(right)) return turnR;

        Pos backward = map.GetBackward;
        if (IsOnPlayer(backward)) return RandomChoice(turnL, turnR);

        Pos left2 = map.dir.GetLeft(left);
        bool isLeftMovable = map.LeftTile.IsViewOpen;
        if (IsOnPlayer(left2) && isLeftMovable) return turnL;

        Pos right2 = map.dir.GetRight(right);
        bool isRightMovable = map.RightTile.IsViewOpen;
        if (IsOnPlayer(right2) && isRightMovable) return turnR;

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
            // Turn if forward unmovable and left or right movable
            if (isLeftMovable) return turnL;
            if (isRightMovable) return turnR;

            // Turn if backward movable
            if (map.BackwardTile.IsViewOpen)
            {
                return RandomChoice(turnL, turnR);
            }
        }

        // Idle if unmovable
        return null;
    }

    public override void InputIced(float duration)
    {
        ClearAll();
        Interrupt(new FlyingIcedFall(target, duration, 25f));
        commander.EnqueueCommand(wakeUp);
        return;
    }
}
