using UnityEngine;

[RequireComponent(typeof(RabbitAnimator))]
[RequireComponent(typeof(RabbitAnimFX))]
[RequireComponent(typeof(RabbitEffect))]
public class RabbitAIInput : EnemyAIInput
{
    protected ICommand idle;
    protected ICommand wondering;
    protected ICommand jumpAttack;
    protected ICommand somersault;
    protected ICommand wakeUp;

    protected override void SetCommands()
    {
        die = new EnemyDie(target, 56f);
        wakeUp = new RabbitWakeUp(target, 80f);
        idle = new EnemyIdle(target, 32f);
        wondering = new RabbitWondering(target, 64f);
        moveForward = new EnemyForward(target, 40f);
        turnL = new EnemyTurnAnimL(target, 8f);
        turnR = new EnemyTurnAnimR(target, 8f);
        attack = new RabbitJumpAttack(target, 80f);
        jumpAttack = new RabbitLongJumpAttack(target, 80f);
        somersault = new RabbitSomersault(target, 80f);
    }
    public void CancelWondering()
    {
        (target.anim as RabbitAnimator).wondering.Bool = false;
        if (commander.currentCommand is RabbitWondering) commander.Cancel();
    }

    protected override ICommand GetCommand()
    {
        var currentCommand = commander.currentCommand;

        Pos forward = mobMap.GetForward;
        Pos left = mobMap.GetLeft;
        Pos right = mobMap.GetRight;
        Pos backward = mobMap.GetBackward;

        bool isForwardMovable = mobMap.IsMovable(forward);
        bool isLeftMovable = mobMap.IsMovable(left);
        bool isRightMovable = mobMap.IsMovable(right);
        bool isBackwardMovable = mobMap.IsMovable(backward);

        if (IsOnPlayer(left))
        {
            // Turn left to player if player found at left and not cowering
            if (currentCommand != turnR) return turnL;
            else if (isForwardMovable) return moveForward;
        }

        if (IsOnPlayer(right))
        {
            // Turn right to player if player found at right and not cowering
            if (currentCommand != turnL) return turnR;
            else if (isForwardMovable) return moveForward;
        }

        // Try cowering if player is on backward
        if (IsOnPlayer(backward)) return isForwardMovable ? moveForward : RandomChoice(turnL, turnR);

        // Attack or Somersault if player found at forward
        if (IsOnPlayer(forward))
        {
            if (isBackwardMovable) return RandomChoice(idle, attack, somersault);

            // Sometimes try cowering if backward isn't movable
            if (isLeftMovable && isRightMovable) return RandomChoice(wondering, attack, turnL, turnR);
            if (isLeftMovable) return RandomChoice(wondering, attack, turnL);
            if (isRightMovable) return RandomChoice(wondering, attack, turnR);
            return RandomChoice(wondering, attack);
        }

        Pos backward2 = mobMap.dir.GetBackward(backward);
        Pos left2 = mobMap.dir.GetLeft(left);
        Pos right2 = mobMap.dir.GetRight(right);

        // Turn to player if player is found in 2 tile distance if able to reach player
        if (IsOnPlayer(backward2) && isBackwardMovable) return RandomChoice(turnL, turnR);
        if (IsOnPlayer(left2) && isLeftMovable) return RandomChoice(turnL, moveForward);
        if (IsOnPlayer(right2) && isRightMovable) return RandomChoice(turnR, moveForward);

        Pos forward2 = mobMap.dir.GetForward(forward);

        // Move forward or jump attack if player found in 2 tile distance front
        if (IsOnPlayer(forward2) && isForwardMovable)
        {
            // Sometimes try cowering if left or right is movable
            if (isLeftMovable && isRightMovable) return RandomChoice(moveForward, jumpAttack, turnL, turnR);
            if (isLeftMovable) return RandomChoice(moveForward, jumpAttack, turnL);
            if (isRightMovable) return RandomChoice(moveForward, jumpAttack, turnR);
            return jumpAttack;
        }

        // Move forward if player found in front
        if (mobMap.IsPlayerFound(forward) && isForwardMovable) return moveForward;

        // Wonder if unmovable
        return MoveForwardOrTurn(isForwardMovable, isLeftMovable, isRightMovable, isBackwardMovable) ?? wondering;
    }

    public override ICommand InputIced(float duration)
    {
        if (commander.currentCommand is RabbitAttack && commander.currentCommand.RemainingTimeScale > 0.3f)
        {
            ClearAll();
            ICommand iced = new RabbitIcedFall(target, duration, 25f);
            Interrupt(iced);
            commander.EnqueueCommand(wakeUp);
            return iced;
        }

        return base.InputIced(duration);
    }
}
